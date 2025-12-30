#include "upgrade.h"
#include "stdint.h"
#include "flash.h"
#include "global_cfg.h"
#include "stdbool.h"
#include "crc.h"
#include "string.h"

typedef struct
{
    uint32_t start_address;
    uint32_t cur_address;
    uint16_t check_idx; // 当前包索引 用于校验下发数据是否连续
    uint16_t cur_idx;   // 当前包索引 以1开始
    uint16_t toltal_packet_num;
    uint8_t flag;
    fw_header_t *hdr;
    uint32_t crc;
    uint32_t remain;
    uint8_t isDelaySiwtch;
    uint32_t delayStartTime;
    bootmode_t bootmode;
} upgrade_t;

upgrade_t upgrade_ctl;

typedef struct
{
    uint16_t cur_idx;
    uint16_t toltal_packet_num;
} upgrade_packet;

typedef struct
{
    uint16_t cur_idx;
    uint16_t toltal_packet_num;
    uint32_t size; // 32字节为单位
    uint32_t crc;  // 校验
} upgrade_first_packet;



//uint32_t crc_test = 0;
void upgrade(uint8_t *data, uint16_t len)
{
    int update_len = (len - sizeof(upgrade_packet));
    if (update_len <= 0)
    {
        return;
    }
    upgrade_packet *p = data;
    uint8_t isLastPacket = (upgrade_ctl.toltal_packet_num == upgrade_ctl.check_idx + 1);
    if (upgrade_ctl.flag == FLAG_OTA_IDLE || upgrade_ctl.flag == FLAG_OTA_LAST_DONE || p->cur_idx == 1)
    {
        upgrade_ctl.toltal_packet_num = p->toltal_packet_num;
        upgrade_ctl.check_idx = 0;
        upgrade_ctl.cur_idx = 1;
        upgrade_ctl.cur_address = upgrade_ctl.start_address;
        isLastPacket = (upgrade_ctl.toltal_packet_num == upgrade_ctl.cur_idx);
        upgrade_ctl.flag = FLAG_OTA_RUNNING;
    }
    else if (upgrade_ctl.toltal_packet_num != p->toltal_packet_num || upgrade_ctl.check_idx + 1 != p->cur_idx || update_len % 32)
    {
        if (isLastPacket)
        {
            upgrade_ctl.flag = FLAG_OTA_LAST_CHECK_ERR;
        }
        else
        {
            upgrade_ctl.flag = FLAG_OTA_CUR_CHECK_ERR;
        }
        goto fail;
    }
    upgrade_ctl.flag = FLAG_OTA_RUNNING;
    upgrade_ctl.cur_idx = p->cur_idx;
    int rslt = 0;
    if (p->cur_idx == 1) // 第一包数据
    {
        update_len = (len - sizeof(upgrade_first_packet));
        rslt =  FLASH_Write(upgrade_ctl.cur_address, data + sizeof(upgrade_first_packet), update_len);
        upgrade_first_packet *p1 = data;
        upgrade_ctl.hdr->fw_size = p1->size;
        upgrade_ctl.remain = p1->size;
        upgrade_ctl.hdr->fw_crc = p1->crc;
        int crc_size = upgrade_ctl.remain < update_len ? (upgrade_ctl.remain + 3) / 4 : update_len / 4;
        upgrade_ctl.crc = HAL_CRC_Calculate(&hcrc, (uint32_t *)(data + sizeof(upgrade_first_packet)), crc_size);
    }
    else
    {
        rslt =  FLASH_Write(upgrade_ctl.cur_address, data + sizeof(upgrade_packet), update_len);
        int crc_size = upgrade_ctl.remain < update_len ? (upgrade_ctl.remain + 3) / 4 : update_len / 4;
        upgrade_ctl.crc = HAL_CRC_Accumulate(&hcrc, (uint32_t *)(data + sizeof(upgrade_packet)), crc_size);
    }
    if (rslt != 0) // 写入失败
    {
        if (isLastPacket)
        {
            upgrade_ctl.flag = FLAG_OTA_LAST_FAILED;
        }
        else
        {
            upgrade_ctl.flag = FLAG_OTA_CUR_FALIED;
        }
        goto fail;
    }
    else
    {
        if (isLastPacket)
        {
            if (upgrade_ctl.crc != upgrade_ctl.hdr->fw_crc)
            {
                upgrade_ctl.flag = FLAG_OTA_LAST_CHECK_ERR;
                goto fail;
            }
            mem_cfg.active_app = upgrade_ctl.start_address;
            mem_cfg.pending_state = PSTATE_PENDING;
            write_cfg();
            // NVIC_SystemReset(); // 重启进入 bootloader 检查升级结果
            upgrade_ctl.flag = FLAG_OTA_LAST_DONE;
            upgrade_ctl.cur_address = upgrade_ctl.start_address;
            upgrade_ctl.check_idx = 0;
            upgrade_ctl.cur_idx = 1;
            upgrade_ctl.toltal_packet_num = 0;
            // crc_test = HAL_CRC_Calculate(&hcrc, (uint32_t *)(upgrade_ctl.cur_address), (upgrade_ctl.hdr->fw_size+3)/4);
        }
        else
        {
            upgrade_ctl.flag = FLAG_OTA_CUR_DONE;
            upgrade_ctl.cur_address += update_len;
            upgrade_ctl.remain -= update_len;
            upgrade_ctl.check_idx++;
        }
    }
    return;
fail:
    if (mem_cfg.active_app != mem_cfg_mirror.active_app)
    {
        memcpy(&mem_cfg, &mem_cfg_mirror, sizeof(mem_cfg));
        write_cfg();
    }
}

void upgrade_ini()
{
    upgrade_ctl.flag = FLAG_OTA_IDLE;
    upgrade_ctl.isDelaySiwtch = 0;
    if (mem_cfg_mirror.active_app == mem_cfg_mirror.app1_address)
    {
        upgrade_ctl.start_address = mem_cfg_mirror.app2_address;
        upgrade_ctl.hdr = &mem_cfg.app2_hdr;
    }
    else
    {
        upgrade_ctl.start_address = mem_cfg_mirror.app1_address;
        upgrade_ctl.hdr = &mem_cfg.app1_hdr;
    }
    upgrade_ctl.cur_address = upgrade_ctl.start_address;
    upgrade_ctl.check_idx = 0;
    upgrade_ctl.cur_idx = 1;
    upgrade_ctl.toltal_packet_num = 0;
}

void get_upgrade_status(upgrade_status_t *s)
{
    s->flag = upgrade_ctl.flag;
    s->cur_idx = upgrade_ctl.cur_idx;
}

uint8_t get_upgrade_result()
{
    // NVIC_SystemReset(); // 重启进入 bootloader 检查升级结果
    return mem_cfg.pending_state;
}

uint8_t get_bootmode()
{
    if (upgrade_ctl.isDelaySiwtch)
    {
        return BT_Switching;
    }
    return upgrade_ctl.bootmode;
}

 
void set_boot_bootmode(uint8_t mode)
{
    if (upgrade_ctl.isDelaySiwtch) // Return if a switch is already pending
    {
        return;
    }
    if ((mode == BT_Active && upgrade_ctl.bootmode == BT_App1) ||
        (mode == BT_Active && upgrade_ctl.bootmode == BT_App2)) // No switch needed
    {
        return;
    }
    if (upgrade_ctl.bootmode != mode)
    {
        upgrade_ctl.isDelaySiwtch = 1;
        upgrade_ctl.bootmode = mode;
        upgrade_ctl.delayStartTime = GET_TickCount;
    }
}


void delay_swith_boot_mode()
{
    if (upgrade_ctl.isDelaySiwtch &&
        (GET_TickCount - upgrade_ctl.delayStartTime) >= 50)
    {
        switch (upgrade_ctl.bootmode)
        {
        case BT_Boot:
            mem_cfg.pending_state = PSTATE_BOOT_UP;
            write_cfg();
            break;
        case BT_Active:
            break;
        case BT_App1:
            mem_cfg.active_app = mem_cfg.app1_address;
            write_cfg();
            break;
        case BT_App2:
            mem_cfg.active_app = mem_cfg.app2_address;
            write_cfg();
            break;
        default:
            break;
        }
        // write_cfg();
        NVIC_SystemReset();
    }
}

#if 0

void upgrade_check()
{
    read_cfg();
    memcpy(&mem_cfg_mirror, &mem_cfg, sizeof(mem_cfg));
    if (mem_cfg.pending_state == PSTATE_APP_CHECK)
    {
        // 告知 bootloader 升级成功（写入 CFG）
        set_update_result();
        // 可选择立即复位，让 bootloader 用新的状态重启后直接跳入 app
        NVIC_SystemReset();
    }
}
#endif