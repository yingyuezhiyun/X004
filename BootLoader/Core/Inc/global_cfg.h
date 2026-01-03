// #ifndef __GLOBAL_CFG_H__
// #define __GLOBAL_CFG_H__

#pragma once

#include "stdlib.h"
#include "stdint.h"
#include "usart.h"
#include "stdbool.h"

// #define CFG_ADDR FLASH_BANK2_BASE
// #define BL_ADDR FLASH_BANK1_BASE
// #define APP1_ADDR (FLASH_BANK1_BASE + 0x20000)
// #define APP2_ADDR (FLASH_BANK2_BASE + 0x20000)

/*
BootLoader:	0x8000000		128k
APP1:		0x8020000		384k
CFG:		0x8080000		384k
APP2:		0x80E0000		128k
*/

#define BL_ADDR FLASH_BANK1_BASE
#define APP1_ADDR (FLASH_BANK1_BASE + 0x20000)
#define APP2_ADDR (FLASH_BANK1_BASE + 0x80000)
// #define CFG_ADDR FLASH_BANK2_BASE
#define CFG_ADDR (FLASH_BANK1_BASE + 0xE0000)

// 固件头定义，存放在 CFG 区
#define FW_MAGIC 0xA5A5A5A5U

typedef struct
{
    uint32_t magic;   // 固件签名
    uint32_t fw_size; // 固件大小（字节）
    uint32_t fw_crc;  // 固件 CRC32
    //uint32_t entry;   // 可选 entry 地址 (0 表示使用 app base + 4)
} fw_header_t;

// pending state for upgrades
typedef enum
{
    PSTATE_NONE = 0,
    PSTATE_PENDING = 1,
    PSTATE_APPLIED = 2,
    PSTATE_FAILED = 3,
    PSTATE_APP_CHECK = 4, // bootloader waiting for app to confirm (watchdog-protected)
    PSTATE_BOOT_UP = 5,   // bootloader upgrade in progress
} pending_state_t;

typedef enum
{
    BT_Boot,
    BT_App
} bootmode_t;

typedef struct
{
   
    uint32_t bootloader_address;
    uint32_t app_address;
    uint32_t cfg_address;
    pending_state_t pending_state;    
    fw_header_t app_hdr;

} mem_cfg_t;

extern mem_cfg_t mem_cfg;



#define GET_TickCount HAL_GetTick()

#define INFO_PRINT (1)

// 跳过 CFG 校验
// #define BOOT_DEBUG_SKIP_CFG_CHECK


enum
{
    FLAG_OTA_IDLE,
    FLAG_OTA_RUNNING,
    FLAG_OTA_LAST_DONE,
    FLAG_OTA_LAST_CHECK_ERR,
    FLAG_OTA_LAST_FAILED,
    FLAG_OTA_CUR_DONE,
    FLAG_OTA_CUR_CHECK_ERR,
    FLAG_OTA_CUR_FALIED
};


#define Delay_ms(x) HAL_Delay(x)


#pragma pack(1)

#define CLI_RX_BUFF 300
typedef struct
{

    volatile uint16_t usart_tail;
    volatile uint8_t usart_pktcplt; // 接收完整一包数据
    volatile uint8_t test;
    char rxbuf[CLI_RX_BUFF];

} cli_para_t;

extern cli_para_t cli_para;

// #endif