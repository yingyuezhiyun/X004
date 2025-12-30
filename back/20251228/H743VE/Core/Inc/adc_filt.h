#pragma once

#include "global_cfg.h"

#define ADC_BUFFER_SIZE 7 // 缓冲区大小，根据需要调整
#define ADC_WINDOW_SIZE 7 // 默认滤波窗口大小（每个缓冲区可覆盖此默认值）
#define ADC_FILT_LEN 2    // 默认去掉的最大/最小数量

// 定义环形缓冲区结构体
typedef struct
{
    uint16_t buffer[ADC_BUFFER_SIZE]; // 数据缓冲区
    uint8_t write_index;             // 写入索引
    uint8_t read_index;              // 读取索引
    uint8_t count;                   // 当前有效数据个数
    uint8_t window_size;             // 本缓冲区使用的窗口大小（<= ADC_BUFFER_SIZE）
    uint8_t filt_len;                // 去掉的最大/最小数量
    uint16_t *ext_buffer;            // 指向外部静态缓冲区（非 NULL 则使用）
    uint8_t buffer_capacity;         // 当前缓冲区的物理容量（内置或外部）
} CircularBuffer;

extern void init_buffer(CircularBuffer *cb);
extern void init_buffer_with_params(CircularBuffer *cb, uint8_t window_size, uint8_t filt_len);
extern void init_buffer_with_external(CircularBuffer *cb, uint16_t *ext_buf, uint8_t ext_capacity, uint8_t window_size, uint8_t filt_len);
extern void init_cbuffers();
extern void adc_write_data_to_buff(CircularBuffer *cb, uint16_t data);
extern uint16_t calculate_moving_average(CircularBuffer *cb);

extern CircularBuffer adc_cb[];
