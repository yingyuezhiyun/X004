#include "usart.h"
#include "string.h"
#include "main.h"
#include "protocol_comm.h"
#include "global_cfg.h"
#include "usart.h"
#include "upgrade.h"




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

// void uart_send(UART_HandleTypeDef *huart, uint8_t cmd, void *data, uint8_t dataLen)
// {
//     static uint8_t UartTxBuff[MAX_SIZE];//共用一个发送缓存 极端情况下可能会资源冲突
//     min_cmd_t *s = UartTxBuff;
//     s->head = CMD_HEAD;
//     s->sendID = MCU_ID;
//     s->revID = PC_ID;
//     s->cmd = cmd;
//     s->len = dataLen;
//     if (dataLen > 0)
//     {
//         memcpy((UartTxBuff + 6), data, dataLen);
//     }
//     UartTxBuff[6 + dataLen] = sum_crc(data, dataLen);
//     UartTxBuff[6 + dataLen + 1] = CMD_TAIL & 0xff;
//     UartTxBuff[6 + dataLen + 2] = CMD_TAIL >> 8;
//     uint8_t length = dataLen + sizeof(min_cmd_t);
//     // HAL_UART_Transmit(&huart1, UartTxBuff, length, 0xfff);
//     HAL_UART_Transmit_DMA(huart, UartTxBuff, length);
// }

// void send_ack(UART_HandleTypeDef *huart, uint8_t cmd, void *data, uint8_t dataLen)
// {
//     if (CheckUartReady(huart))
//     {
//         uart_send(huart, cmd, data, dataLen);
//     }
// }

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



