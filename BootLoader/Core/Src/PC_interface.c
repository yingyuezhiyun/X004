#include "main.h"
#include "global_cfg.h"
#include "usart.h"
#include "string.h"
#include "PC_interface.h"
#include "upgrade.h"

#define MAX_SIZE (256)
#define CMD_HEAD (0x545A)
#define CMD_TAIL (0xFE5A)
#define PC_ID (0x14)
#define MCU_ID (0x1c)

#pragma pack(1)
typedef struct
{
    uint16_t head;
    uint8_t sendID;
    uint8_t revID;
    uint8_t cmd;
    uint8_t len;
    uint8_t crc;
    uint16_t tail;
} min_cmd_t;
#pragma unpack()

enum
{
    P_S_UPGRADE = 0x76,        /* 在线程序升级 */
    P_G_UPGRADE = 0x77,        /* 查询升级程序状态 */
    P_Clear_Err = 0x11,        /* 清空错误 */ 
    P_S_BOOTMODE = 0x13,       /* 设置BOOT模式 */
    P_G_BOOTMODE = 0x14,       /* 查询BOOT模式 */
};

enum
{
    M_UPGRADE = 0xD0,        /* 反馈在线程序升级状态 */ 
    M_BOOT_MODE = 0xe4,      /* 处于BOOT模式 反馈 */
};

uint8_t sum_crc(uint8_t *data, uint8_t len)
{
    uint8_t sum = 0;
    for (size_t i = 0; i < len; i++)
    {
        sum += data[i];
    }
    return sum;
}

void uart1_send(uint8_t cmd, void *data, uint8_t dataLen)
{
    static uint8_t UartTxBuff[MAX_SIZE];
    min_cmd_t *s = UartTxBuff;
    s->head = CMD_HEAD;
    s->sendID = MCU_ID;
    s->revID = PC_ID;
    s->cmd = cmd;
    s->len = dataLen;
    if (dataLen > 0)
    {
        memcpy((UartTxBuff + 6), data, dataLen);
    }
    UartTxBuff[6 + dataLen] = sum_crc(data, dataLen);
    UartTxBuff[6 + dataLen + 1] = CMD_TAIL & 0xff;
    UartTxBuff[6 + dataLen + 2] = CMD_TAIL >> 8;
    uint8_t length = dataLen + sizeof(min_cmd_t);
    // HAL_UART_Transmit(&huart1, UartTxBuff, length, 0xfff);
    HAL_UART_Transmit_DMA(&huart1, UartTxBuff, length);
}

void pc_send_ack(uint8_t cmd, void *data, uint8_t dataLen)
{
    if (CheckUartReady())
    {
        uart1_send(cmd, data, dataLen);
    }
}

#define PC_ACK(cmd, data) pc_send_ack(cmd, &data, sizeof(data))

#define PC_ACK_INT16(cmd, data)                    \
    {                                              \
        int16_t data1 = (int16_t)round(data);      \
        pc_send_ack(cmd, &data1, sizeof(int16_t)); \
    }

#define PC_ACK_UINT16(cmd, data)                    \
    {                                               \
        uint16_t data1 = (uint16_t)round(data);     \
        pc_send_ack(cmd, &data1, sizeof(uint16_t)); \
    }

void ClearErrs()
{
    
    upgrade_ini();
}

void ack_upgrade_status()
{
    upgrade_status_t status;
    get_upgrade_status(&status);
    PC_ACK(M_UPGRADE, status);
}

void ack_boot_mode()
{
    uint8_t status = get_bootmode();
    PC_ACK(M_BOOT_MODE, status);
}

void exec_commands(uint8_t cmd, uint8_t *data, size_t data_len)
{
    switch (cmd)
    {
        case P_S_UPGRADE:                upgrade(data, data_len);                                                    break;
        case P_G_UPGRADE:                ack_upgrade_status();                                                       break;
        case P_Clear_Err:                ClearErrs();                                                                break; 
        case P_S_BOOTMODE:               set_boot_bootmode(*(uint8_t *)data);ack_boot_mode();                        break;
        case P_G_BOOTMODE:               ack_boot_mode();                                                            break;
        default:                         ack_boot_mode();                                                            break;
    }
}

void pc_parse_and_execute_command2()
{

    // 拷贝数据后 再进行处理？
    if (uart1_para.pktcplt && uart1_para.tail >= sizeof(min_cmd_t))
    {
        // Disable_UART1_Receive();
        uint16_t cmd_pos = 0;
        for (size_t i = 0; i < uart1_para.tail; i++)
        {
            min_cmd_t *data = (min_cmd_t *)(uart1_para.rxbuf + i);
            if (data->head == CMD_HEAD &&                                                                         /* 帧头校验 */
                data->sendID == PC_ID &&                                                                          /* 发送id校验 */
                data->revID == MCU_ID &&                                                                          /* 接收id校验 */
                uart1_para.tail - cmd_pos >= sizeof(min_cmd_t) + data->len &&                                 /* 长度满足要求 */
                sum_crc(uart1_para.rxbuf + i + 6, data->len) == *(uint8_t *)(uart1_para.rxbuf + i + 6 + data->len) && /* 数据和校验 */
                *(uint16_t *)(uart1_para.rxbuf + i + 7 + data->len) == CMD_TAIL                                     /* 帧尾校验 */
            )
            {
                exec_commands(data->cmd, (uint8_t *)data + 6, data->len);
                i += sizeof(min_cmd_t) + data->len - 1;
                cmd_pos = i + 1;
            }
        }

        if (cmd_pos < uart1_para.tail)
        {
            memcpy(uart1_para.rxbuf, uart1_para.rxbuf + cmd_pos, uart1_para.tail - cmd_pos);
            uart1_para.tail -= (cmd_pos);
        }
        else
        {
            uart1_para.tail = 0;
        }
        if (uart1_para.tail == CLI_RX_BUFF)
        {
            uart1_para.tail = 0;
        }
        uart1_para.pktcplt = 0;
        // Enable_UART1_Receive();
    }

    // UART1_Check();
}
