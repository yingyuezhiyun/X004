
#include "main.h"

#include "cmsis_os.h"
#include "adc.h"
#include "dma.h"
#include "i2c.h"
#include "spi.h"
#include "tim.h"
#include "usart.h"
#include "gpio.h"

#include "adc_filt.h"




/**ADC1 GPIO Configuration
0       PA0     ------> ADC1_INP16       MAX_Cur1_Pin                       
1       PA1     ------> ADC1_INP17       MAX_Vol1_Pin                       
2       PA2     ------> ADC1_INP14       MAX_Cur2_Pin                       
3       PA3     ------> ADC1_INP15       MAX_Vol2_Pin                       
*/
/**ADC2 GPIO Configuration
4        PA4     ------> ADC2_INP18      PCB_TEMP1_Pin                      
5        PA5     ------> ADC2_INP19      PCB_TEMP2_Pin                      
6        PA6     ------> ADC2_INP3       PCB_TEMP3_Pin                      
7        PA7     ------> ADC2_INP7       PCB_TEMP4_Pin                      
8        PC4     ------> ADC2_INP4       OPT_PW_Pin                                          
9        PC5     ------> ADC2_INP8       OPT_TEMP_Pin                                        
10       PB0     ------> ADC2_INP9       ALL_Cur_Pin                                         
11       PB1     ------> ADC2_INP5       SYS_Vol_Pin                                         
*/
/**ADC3 GPIO Configuration
12       PC0     ------> ADC3_INP10     TEC1_Cur_Pin                       
13       PC1     ------> ADC3_INP11     TEC2_Cur_Pin                       
14       PC2_C   ------> ADC3_INP0      TEC3_Cur_Pin                       
15       PC3_C   ------> ADC3_INP1      TEC4_Cur_Pin                       
*/
CircularBuffer adc_cb[16];



// 为通道 10 提供更大的静态缓冲区（无需 malloc）
static uint16_t adc10_ext_buffer[50];
// 说明：ext_capacity = 50, window = 40, filt_len = 5


// 初始化环形缓冲区
void init_buffer(CircularBuffer *cb) {
    init_buffer_with_params(cb, ADC_WINDOW_SIZE, ADC_FILT_LEN);
}

// 带参数的初始化，允许为单个缓冲区设置窗口大小和去掉的最大/最小数量
void init_buffer_with_params(CircularBuffer *cb, uint8_t window_size, uint8_t filt_len) {
    if (window_size == 0 || window_size > ADC_BUFFER_SIZE) {
        // 限制在合理范围内
        window_size = ADC_WINDOW_SIZE;
    }
    cb->write_index = 0;
    cb->read_index = 0;
    cb->count = 0;
    cb->window_size = window_size;
    cb->filt_len = filt_len;
    cb->ext_buffer = NULL;
    cb->buffer_capacity = ADC_BUFFER_SIZE;
    for (int i = 0; i < ADC_BUFFER_SIZE; i++) {
        cb->buffer[i] = 0;  // 将缓冲区的所有值初始化为0
    }
}

// 使用外部静态缓冲区初始化（不会 malloc），ext_buf 必须指向外部分配的数组
void init_buffer_with_external(CircularBuffer *cb, uint16_t *ext_buf, uint8_t ext_capacity, uint8_t window_size, uint8_t filt_len)
{
    if (ext_buf == NULL || ext_capacity == 0) {
        // 回退到默认初始化
        init_buffer_with_params(cb, window_size, filt_len);
        return;
    }
    if (window_size == 0 || window_size > ext_capacity) {
        window_size = (ext_capacity >= ADC_WINDOW_SIZE) ? ADC_WINDOW_SIZE : ext_capacity;
    }
    cb->write_index = 0;
    cb->read_index = 0;
    cb->count = 0;
    cb->window_size = window_size;
    cb->filt_len = filt_len;
    cb->ext_buffer = ext_buf;
    cb->buffer_capacity = ext_capacity;
    // 将外部缓冲区置零
    for (int i = 0; i < ext_capacity; i++) {
        ext_buf[i] = 0;
    }
}

void init_cbuffers()
{
    for (size_t i = 0; i < 16; i++)
    {
        init_buffer(&adc_cb[i]);
    }
    // 覆盖通道 10 使用外部静态缓冲并设置窗口与滤波参数
    init_buffer_with_external(&adc_cb[10], adc10_ext_buffer, 50, 40, 5);
}

// 向环形缓冲区写入数据
void adc_write_data_to_buff(CircularBuffer *cb, uint16_t data) {
    uint8_t cap = cb->ext_buffer ? cb->buffer_capacity : ADC_BUFFER_SIZE;
    if (cb->ext_buffer) {
        cb->ext_buffer[cb->write_index] = data;
    } else {
        cb->buffer[cb->write_index] = data;
    }
    cb->write_index = (cb->write_index + 1) % cap;

    if (cb->count < cap) {
        cb->count++;  // 如果缓冲区还没满，增加有效数据个数
    } else {
        cb->read_index = (cb->read_index + 1) % cap;  // 缓冲区已满，覆盖最旧的数据
    }
}


void bubble_sort(uint16_t data[], uint8_t size) {
    for (uint8_t i = 0; i < size - 1; i++) {
        for (uint8_t j = 0; j < size - 1 - i; j++) {
            if (data[j] > data[j + 1]) {
                uint16_t temp = data[j];
                data[j] = data[j + 1];
                data[j + 1] = temp;
            }
        }
    }
}


// 计算滑动平均
uint16_t calculate_moving_average(CircularBuffer *cb) {
    if (cb->count == 0) {
        // 没有数据，无法计算
        return 0;
    }
    float sum = 0;
    uint8_t window = cb->window_size;
    uint8_t filt_len = cb->filt_len;
    uint8_t cap = cb->ext_buffer ? cb->buffer_capacity : ADC_BUFFER_SIZE;
    uint8_t valid_data_count = (cb->count < window) ? cb->count : window;

    uint16_t data[valid_data_count];

    // 从缓冲区获取有效数据（从最新往前取）
    uint8_t idx = cb->write_index;
    for (uint8_t j = 0; j < valid_data_count; j++) {
        idx = (idx == 0) ? (cap - 1) : (idx - 1);
        data[j] = cb->ext_buffer ? cb->ext_buffer[idx] : cb->buffer[idx];
    }
    // 排序数据
    bubble_sort(data, valid_data_count);

    if (2 * filt_len >= valid_data_count) {
        return 0; // 去掉最大/最小后没有数据
    }

    uint8_t remaining_data_count = valid_data_count - 2 * filt_len;

    // 从去除最小值和最大值后的数据中计算总和
    for (uint8_t k = filt_len; k < valid_data_count - filt_len; k++) {
        sum += data[k];
    }

    return (uint16_t)(sum / remaining_data_count); // 返回平均值

}