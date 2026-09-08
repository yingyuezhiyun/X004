#include "usart.h"
#include "string.h"
#include "main.h"
#include "cmsis_os2.h"
#include "protocol_comm.h"
#include "global_cfg.h"
#include "usart.h"

char Version[] = "X004_001";

uint64_t BuildTime = 202509112205;

char Version2[] = {0, 0, 1, 9};//V0.0.1.9

uint8_t sum_crc(uint8_t *data, uint8_t len)
{
    uint8_t sum = 0;
    for (size_t i = 0; i < len; i++)
    {
        sum += data[i];
    }
    return sum;
}

int CheckUartReady(UART_HandleTypeDef *huart)
{
    HAL_DMA_StateTypeDef res = HAL_DMA_GetState(huart->hdmatx);
    uint8_t loop = 0;
    while (res != HAL_DMA_STATE_READY && loop < 50)
    {
        Delay_ms(1);
        loop++;
        res = HAL_DMA_GetState(huart->hdmatx);
    }
    if (res != HAL_DMA_STATE_READY)
    {
        return 0;
    }
    return 1;
}



void parse_and_execute_command(uart_para_t *uart_para, CommandFunction exec_commands)
{
    // uart_para_t *uart_para = &uart1_para;
    // 拷贝数据后 再进行处理？
    if (uart_para->pktcplt && uart_para->tail >= sizeof(min_cmd_t))
    {
        // Disable_UART1_Receive();
        uint16_t cmd_pos = 0;
        for (size_t i = 0; i < uart_para->tail; i++)
        {
            min_cmd_t *data = (min_cmd_t *)(uart_para->rxbuf + i);
            if (data->head == CMD_HEAD &&                                                                             /* 帧头校验 */
                data->sendID == PC_ID &&                                                                              /* 发送id校验 */
                data->revID == MCU_ID &&                                                                              /* 接收id校验 */
                uart_para->tail - cmd_pos >= sizeof(min_cmd_t) + data->len &&                                         /* 长度满足要求 */
                sum_crc(uart_para->rxbuf + i + 6, data->len) == *(uint8_t *)(uart_para->rxbuf + i + 6 + data->len) && /* 数据和校验 */
                *(uint16_t *)(uart_para->rxbuf + i + 7 + data->len) == CMD_TAIL                                       /* 帧尾校验 */
            )
            {
                exec_commands(uart_para->huart, data->cmd, (uint8_t *)data + 6, data->len);
                i += sizeof(min_cmd_t) + data->len - 1;
                cmd_pos = i + 1;
            }
        }
        if (cmd_pos < uart_para->tail)
        {
            memcpy(uart_para->rxbuf, uart_para->rxbuf + cmd_pos, uart_para->tail - cmd_pos);
            uart_para->tail -= (cmd_pos);
        }
        else
        {
            uart_para->tail = 0;
        }
        if (uart_para->tail == CLI_RX_BUFF)
        {
            uart_para->tail = 0;
        }
        uart_para->pktcplt = 0;
        // Enable_UART1_Receive();
    }
    // UART1_Check();
}

void set_pulse_width(uint16_t width)
{
    set_param.Pulse_para.Width = width;
    CalcPulse_SPWMParam();
    CalcPulse_NORParam();
}

void set_Q_delay(uint16_t delay)
{
    set_param.T_Q.delay = delay;
    CalcPulse_SPWMParam();
    CalcPulse_NORParam(); 
}

void set_nor_freq(uint16_t freq)
{
    set_param.Pulse_para.Nor_Freq = freq;
    CalcPulse_NORParam();
}


void set_pulse_param(uint8_t *data)
{
    Pulse_param_packet_t *p = data;
    uint16_t sum_pulse_width=0;
    // todo 判断
    if (p->Num > PULSE_MAX_SECTION)
    {
        return;
    }
    for (size_t i = 0; i < p->Num; i++)
    {
        sum_pulse_width += p->Interval[i];
        if (p->Interval[i] > 660 || p->Interval[i] < 240)
        {
            return;
        }
    }
    if (sum_pulse_width > 5000)
    {
        return;
    }
    set_param.Pulse_para.Num = p->Num;
    for (size_t i = 0; i < p->Num; i++)
    {
        set_param.Pulse_para.Interval[i] = p->Interval[i];
    }
    CalcPulse_SPWMParam();
}

Pulse_param_packet_t get_pulse_param()
{
    Pulse_param_packet_t p;
    set_param_t *data = &set_param;
    p.Num = data->Pulse_para.Num;
    for (size_t i = 0; i < p.Num; i++)
    {
        p.Interval[i] = data->Pulse_para.Interval[i];
    }
    return p;
}

void set_pulse_type(uint8_t data)
{
    if (set_param.ld[0].sw == WORK_ON || set_param.ld[1].sw == WORK_ON)
    {
        return;
    }
    if (set_param.TRG_Type == TRG_OUT)
    {
        set_param.Pulse_Type = PULSE_SPWM;//外触发下强制使用SPWM
    }
    else if (data == PULSE_SPWM || data == PULSE_NOR)
    {
        set_param.Pulse_Type = data;
    }
    
}

void set_TRG_type(uint8_t data)
{
    if (set_param.ld[0].sw == WORK_ON || set_param.ld[1].sw == WORK_ON)
    {
        return;
    }
    if (data == TRG_INTER)
    {
        set_param.TRG_Type = data;
        Disable_TRG_IN_DET;
    }
    else if (data == TRG_OUT)
    {
        set_param.Pulse_Type = PULSE_SPWM;
        set_param.TRG_Type = data;
        Enable_TRG_IN_DET;
    }
}

void ClearErrs()
{
    set_param.ld[0].ErrStatus.value = 0;
    set_param.ld[1].ErrStatus.value = 0;
    for (size_t i = 0; i < 4; i++)
    {
        set_param.tec[i].ErrStatus.value = 0;
    }
    memset(&Work_Status,0,sizeof(Work_Status));
    upgrade_ini();
}

