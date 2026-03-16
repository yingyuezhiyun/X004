#include "main.h"
#include "global_cfg.h"
#include "usart.h"
#include "string.h"
#include "PC_interface.h"
#include "upgrade.h"
#include "protocol_comm.h"



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

static void uart_send(UART_HandleTypeDef *huart, uint8_t cmd, void *data, uint8_t dataLen)
{
    static uint8_t UartTxBuff[MAX_SIZE];//共用一个发送缓存 极端情况下可能会资源冲突
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
    HAL_UART_Transmit_DMA(huart, UartTxBuff, length);
}

static void send_ack(UART_HandleTypeDef *huart, uint8_t cmd, void *data, uint8_t dataLen)
{
    if (CheckUartReady(huart))
    {
        uart_send(huart, cmd, data, dataLen);
    }
}

void ClearErrs()
{
    upgrade_ini();
}

static void ack_upgrade_status(UART_HandleTypeDef *huart)
{
    upgrade_status_t status;
    get_upgrade_status(&status);
    PC_ACK(M_UPGRADE, status);
}

static void ack_boot_mode(UART_HandleTypeDef *huart, uint8_t cmd)
{
    uint8_t mode = get_bootmode();
    PC_ACK(cmd, mode);
}

void exec_commands_list1(UART_HandleTypeDef *huart, uint8_t cmd, uint8_t *data, size_t data_len)
{
    switch (cmd)
    {
        case P_S_UPGRADE:                upgrade(data, data_len);                                                    break;
        case P_G_UPGRADE:                ack_upgrade_status(huart);                                                       break;
        case P_Clear_Err:                ClearErrs();                                                                break; 
        case P_S_BOOTMODE:               set_boot_bootmode(*(uint8_t *)data);ack_boot_mode(huart, M_BOOT_MODE);                        break;
        case P_G_BOOTMODE:               ack_boot_mode(huart, M_BOOT_MODE);                                                            break;
        default:                         ack_boot_mode(huart, M_BOOT_MODE);                                                           break;
    }
}

