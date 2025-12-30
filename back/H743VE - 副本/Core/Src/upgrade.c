#include "upgrade.h"
#include "stdint.h"
#include "flash.h"
#include "global_cfg.h"

typedef struct
{
    uint32_t start_address;
    uint32_t cur_address;
    uint16_t cur_idx;
    uint16_t toltal_packet_num;
    uint8_t flag;
} upgrade_t;

upgrade_t upgrade_ctl;

typedef struct
{
    uint16_t cur_idx;
    uint16_t toltal_packet_num;
} upgrade_packet;

void upgrade(uint8_t *data, uint16_t len)
{
    int update_len = (len - sizeof(upgrade_packet));
    if (update_len <= 0)
    {
        return;
    }
    upgrade_packet *p = data;
    uint8_t isLastPacket = (upgrade_ctl.toltal_packet_num == upgrade_ctl.cur_idx + 1);
   
    if (upgrade_ctl.flag == FLAG_OTA_IDLE || upgrade_ctl.flag == FLAG_OTA_LAST_DONE)
    {
        upgrade_ctl.toltal_packet_num = p->toltal_packet_num;
        upgrade_ctl.cur_idx = 0;
        upgrade_ctl.cur_address = upgrade_ctl.start_address;
        isLastPacket = (upgrade_ctl.toltal_packet_num == upgrade_ctl.cur_idx + 1);
        upgrade_ctl.flag = FLAG_OTA_RUNNING;
    }
    else if (upgrade_ctl.toltal_packet_num != p->toltal_packet_num || upgrade_ctl.cur_idx + 1 != p->cur_idx || update_len % 32)
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
    int rslt = FLASH_Write(upgrade_ctl.cur_address, data + sizeof(upgrade_packet), update_len);
    if (rslt != 0)
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

            mem_cfg.current_app_address = upgrade_ctl.start_address;
            write_cfg();
            
            upgrade_ctl.flag = FLAG_OTA_LAST_DONE;
            upgrade_ctl.cur_address = upgrade_ctl.start_address;
            upgrade_ctl.cur_idx = 0;
            upgrade_ctl.toltal_packet_num = 0;
        }
        else
        {
            upgrade_ctl.flag = FLAG_OTA_CUR_DONE;
            upgrade_ctl.cur_address += update_len;
            upgrade_ctl.cur_idx++;
        }
    }
    return;
fail:
    if (mem_cfg.current_app_address != mem_cfg_mirror.current_app_address)
    {
        mem_cfg.current_app_address = mem_cfg_mirror.current_app_address;
        write_cfg();
    }
}

void upgrade_ini()
{
    upgrade_ctl.flag = FLAG_OTA_IDLE;
    if (mem_cfg_mirror.current_app_address == mem_cfg_mirror.app1_address)
    {
        upgrade_ctl.start_address = mem_cfg_mirror.app2_address;
    }
    else
    {
        upgrade_ctl.start_address = mem_cfg_mirror.app1_address;
    }
    upgrade_ctl.cur_address = upgrade_ctl.start_address;
    upgrade_ctl.cur_idx = 0;
    upgrade_ctl.toltal_packet_num = 0;
}

uint8_t get_upgrade_status()
{
    return upgrade_ctl.flag;
}