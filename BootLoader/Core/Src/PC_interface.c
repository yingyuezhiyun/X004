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


void ClearErrs()
{
    upgrade_ini();
}

void ack_upgrade_status(UART_HandleTypeDef *huart)
{
    upgrade_status_t status;
    get_upgrade_status(&status);
    PC_ACK(M_UPGRADE, status);
}



void exec_commands_list1(UART_HandleTypeDef *huart, uint8_t cmd, uint8_t *data, size_t data_len)
{
    switch (cmd)
    {
        case P_S_UPGRADE:                upgrade(data, data_len);                                                    break;
        case P_G_UPGRADE:                ack_upgrade_status(huart);                                                       break;
        case P_Clear_Err:                ClearErrs();                                                                break; 
        case P_S_BOOTMODE:               set_boot_bootmode(*(uint8_t *)data);get_boot_mode(huart,M_BOOT_MODE);                        break;
        case P_G_BOOTMODE:               get_boot_mode(huart,M_BOOT_MODE);                                                            break;
        default:                         get_boot_mode(huart,M_BOOT_MODE);                                                           break;
    }
}

