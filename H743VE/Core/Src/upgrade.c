#include "upgrade.h"
#include "stdint.h"
#include "flash.h"
#include "global_cfg.h"
#include "stdbool.h"
#include "crc.h"
#include "string.h"

typedef struct
{
    bootmode_t bootmode;
} upgrade_t;

upgrade_t upgrade_ctl;





void upgrade_ini()
{
   upgrade_ctl.bootmode = BT_App;
}



void upgrade_check()
{
    read_cfg();
    // SCB->VTOR =/*  FLASH_BASE | */ mem_cfg.active_app; 
    if (mem_cfg.pending_state == PSTATE_APP_CHECK)
    {
        // 告知 bootloader 升级成功（写入 CFG）
        mem_cfg.pending_state = PSTATE_APPLIED;
        write_cfg();
        // 可选择立即复位，让 bootloader 用新的状态重启后直接跳入 app
        NVIC_SystemReset();
    }
}

uint8_t get_bootmode()
{
    return upgrade_ctl.bootmode;
}

void set_boot_bootmode(uint8_t mode)
{
     if (upgrade_ctl.bootmode != mode)
    {
        if (mode == BT_Boot)
        {
            mem_cfg.pending_state = PSTATE_BOOT_UP;
            write_cfg();
        }
        else if ( mem_cfg.pending_state != PSTATE_PENDING)
        {
            mem_cfg.pending_state = PSTATE_PENDING;
            write_cfg();
        }        
        NVIC_SystemReset();
        // upgrade_ctl.bootmode = mode;
        // upgrade_ctl.delayStartTime = GET_TickCount;
    }
}
