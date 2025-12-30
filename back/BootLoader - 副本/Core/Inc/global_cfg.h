// #ifndef __GLOBAL_CFG_H__
// #define __GLOBAL_CFG_H__

#pragma once

#include "stdlib.h"
#include "stdint.h"
#include "usart.h"

#define CFG_ADDR FLASH_BANK2_BASE
#define BL_ADDR FLASH_BANK1_BASE
#define APP1_ADDR (FLASH_BANK1_BASE + 0x20000)
#define APP2_ADDR (FLASH_BANK2_BASE + 0x20000)

/*
BootLoader:	0x8000000		128k
APP1:		0x8020000		384k
CFG:		0x8080000		384k
APP2:		0x80E0000		128k
*/

// #define BL_ADDR FLASH_BANK1_BASE
// #define APP1_ADDR (FLASH_BANK1_BASE + 0x20000)
// #define APP2_ADDR (FLASH_BANK1_BASE + 0x80000)
// // #define CFG_ADDR FLASH_BANK2_BASE
// #define CFG_ADDR (FLASH_BANK1_BASE + 0xE0000)

typedef struct
{
    uint32_t current_app_address;
    uint32_t bootloader_address;
    uint32_t app1_address;
    uint32_t app2_address;
    uint32_t cfg_address;
} mem_cfg_t;



extern mem_cfg_t mem_cfg;


#define INFO_PRINT (1)

// #endif