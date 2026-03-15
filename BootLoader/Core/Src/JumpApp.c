#include "main.h"
#include "crc.h"
#include "memorymap.h"
#include "usart.h"
#include "gpio.h"
#include "stdio.h"
#include "stdint.h"
#include "string.h"
#include "global_cfg.h"
#include "flash.h"
#include "stdarg.h"
#include "JumpApp.h"
#include "iwdg.h"

#if 0
uint32_t crc32_mpeg2_bytes_as_le_words(const uint8_t *data, size_t len)
{
    if (len == 0)
        return 0;

    const uint32_t POLY = 0x04C11DB7U;
    uint32_t crc = 0xFFFFFFFFU;
    size_t words = (len + 3) / 4;
    for (size_t wi = 0; wi < words; ++wi)
    {
        uint32_t w = 0;
        size_t base = wi * 4;
        // Pack bytes into little-endian uint32_t: b0 is LSB
        for (int b = 0; b < 4; ++b)
        {
            size_t idx = base + b;
            uint8_t v = (idx < len) ? data[idx] : 0;
            w |= ((uint32_t)v) << (8 * b);
        }
        // Process bytes in MSB-first order (word >> 24, >>16, >>8, >>0)
        for (int byte_i = 3; byte_i >= 0; --byte_i)
        {
            uint8_t cur = (uint8_t)((w >> (8 * byte_i)) & 0xFFU);
            crc ^= ((uint32_t)cur) << 24;
            for (int bit = 0; bit < 8; ++bit)
            {
                crc = (crc & 0x80000000U) ? (crc << 1) ^ POLY : (crc << 1);
            }
        }
    }
    return crc; // no final xor (matches CRC-32/MPEG-2 used by STM32 HAL default)
}

#endif

static uint32_t crc32_compute_flash(uint32_t addr, uint32_t len)
{

    if (len == 0)
        return 0;
    uint32_t words = (len + 3) / 4; // round up
    uint32_t *p = (uint32_t *)addr; // flash memory mapped
    // HAL_CRC_Calculate resets the CRC calculation and computes over buffer
    return HAL_CRC_Calculate(&hcrc, p, words);
}

void mem_cfg_init(void)
{

    read_cfg(); // flash.c: reads latest appended mem_cfg into mem_cfg
    mem_cfg.bootloader_address = BL_ADDR;
    mem_cfg.app_address = APP1_ADDR;
    mem_cfg.cfg_address = CFG_ADDR;

    // if headers not present (first boot or older format), initialize defaults
    if (mem_cfg.app_hdr.magic != FW_MAGIC )
    {
        
        mem_cfg.app_hdr.magic = FW_MAGIC;
        mem_cfg.app_hdr.fw_size = 0;
        mem_cfg.app_hdr.fw_crc = 0xFFFFFFFFU;
       // mem_cfg.app_hdr.entry = 0;       
        mem_cfg.pending_state = PSTATE_NONE;
        write_cfg();
    }
    // ensure pending_state initialized (older cfg may lack it)
    if (mem_cfg.pending_state > PSTATE_BOOT_UP)
    {
        mem_cfg.pending_state = PSTATE_NONE;
    }
}

/* check_app: vector sanity check + optional CFG header/CRC check */
uint8_t check_app(uint32_t app_addr)
{
    uint8_t result = 1;
    // 1) basic vector table validity
    uint32_t *vt = (uint32_t *)app_addr;
    if (vt == NULL)
    {
        return 0;
    }
    vt = (uint32_t *)(app_addr + 4);
    if (vt == NULL)
    {
        return 0;
    }
    uint32_t sp = *(volatile uint32_t *)app_addr;
    uint32_t reset = *(volatile uint32_t *)(app_addr + 4);
    if (((sp & 0x2FFE0000) != 0x24000000) || (reset == 0xFFFFFFFFU))
    {
        return 0;
    }
/* If BOOT_DEBUG_SKIP_CFG_CHECK is defined, skip header+CRC check (for Keil debug). */
#ifndef BOOT_DEBUG_SKIP_CFG_CHECK
    // 2) if configured, verify header stored in CFG (mem_cfg)
    const fw_header_t *hdr = NULL;
    if (app_addr == mem_cfg.app_address)
        hdr = &mem_cfg.app_hdr;   
    else
    {
        // unknown app region:
        return 0;
    }
    if (hdr->magic != FW_MAGIC)
    {
        return 0;
    }
    if (hdr->fw_size == 0)
    {
        return 0;
    }

    uint32_t calc = crc32_compute_flash(app_addr, hdr->fw_size);
    info_printf("App at 0x%08X: size=0x%X, stored CRC=0x%08X, calc CRC=0x%08X\r\n",
                app_addr, hdr->fw_size, hdr->fw_crc, calc);
    if (calc != hdr->fw_crc)
        return 0;
#endif

    return 1;
}



void Check_Jump_to_APP()
{

    if(mem_cfg.pending_state == PSTATE_BOOT_UP)
    {
        info_printf("Boot-up upgrade in progress. Stay in bootloader.\r\n");
        return;
    }
    if (mem_cfg.pending_state == PSTATE_FAILED)
    {
        return; // failed, stay in bootloader
    }
    
    
#ifdef BOOT_DEBUG_SKIP_CFG_CHECK
    info_printf("Checking apps (header/CRC disabled)\r\n");
#else
    info_printf("Checking apps (header/CRC enabled)\r\n");
#endif
    // if entered APP_CHECK (bootloader started watchdog and jumped
    // to app but didn't receive confirmation), mark it failed and attempt rollback.
    if (mem_cfg.pending_state == PSTATE_APP_CHECK)
    {
        info_printf("Previous APP check timed out. Marking failed.\r\n");
        mem_cfg.pending_state = PSTATE_FAILED; // failed
        write_cfg();
        return;
    }

    // Process pending update first (if marked pending)
    if (mem_cfg.pending_state == PSTATE_PENDING)
    {
        info_printf("Pending update state set for 0x%08X\r\n", mem_cfg.app_address);
        if (check_app(mem_cfg.app_address))
        {
            /* Move to APP_CHECK state: record intention in cfg (flash) so that after
             * a watchdog reset bootloader can detect app didn't confirm and rollback. */
            mem_cfg.pending_state = PSTATE_APP_CHECK;
            write_cfg();

            /* Start watchdog to protect the check window. MX_IWDG1_Init will
             * initialize and start IWDG1 with configured timeout. */
            MX_IWDG1_Init();

            Jump_to_APP(mem_cfg.app_address);
            return;
        }
        else
        {
            info_printf("Pending app invalid. Marking failed.\r\n");
            mem_cfg.pending_state = PSTATE_FAILED; // failed
            mem_cfg.app_hdr.fw_size = 0;           // invalidate
            write_cfg();
            // continue normal checks
        }
    }

    if (check_app(mem_cfg.app_address))
    {
        info_printf("Active APP Check OK: 0x%08X\r\n", mem_cfg.app_address);
        Jump_to_APP(mem_cfg.app_address);
        return;
    }
    info_printf("Active APP Check Failed: 0x%08X\r\n", mem_cfg.app_address);

    info_printf("No valid application found. Stay in bootloader.\r\n");
}

typedef void (*pFunction)(void);
pFunction Jump_To_Application;
void Jump_to_APP(uint32_t app_addr)
{
    uint32_t JumpAddress;
    info_printf("Try to jump to app: %#x\r\n", app_addr);
    Delay_ms(10);
    __set_PRIMASK(1);
    HAL_UART_MspDeInit(&huart1);
    HAL_UART_MspDeInit(&huart2);
    HAL_CRC_MspDeInit(&hcrc);
    HAL_RCC_DeInit();
    JumpAddress = *(volatile uint32_t *)(app_addr + 4);
    Jump_To_Application = (pFunction)JumpAddress;
    __set_MSP(*(volatile uint32_t *)app_addr);
    /* Set VTOR to the application's vector table base (use passed app_addr) */
     SCB->VTOR = /* FLASH_BASE | */ app_addr;
     __DSB();
     __ISB();
    Jump_To_Application();
}

/*
 ******************************************************************************************************
 *    函 数 名: JumpToBootloader
 *    功能说明: 跳转到系统BootLoader
 *    形    参: 无
 *    返 回 值: 无
 ******************************************************************************************************
 */
void JumpToBootloader(void)
{
    uint32_t i = 0;
    void (*SysMemBootJump)(void);        /* 声明一个函数指针 */
    __IO uint32_t BootAddr = 0x1FF09800; /* STM32H7的系统BootLoader地址 */

    /* 关闭全局中断 */
    __set_PRIMASK(1);

    /* 关闭滴答定时器，复位到默认值 */
    SysTick->CTRL = 0;
    SysTick->LOAD = 0;
    SysTick->VAL = 0;

    /* 设置所有时钟到默认状态，使用HSI时钟 */
    HAL_RCC_DeInit();

    /* 关闭所有中断，清除所有中断挂起标志 */
    for (i = 0; i < 8; i++)
    {
        NVIC->ICER[i] = 0xFFFFFFFF;
        NVIC->ICPR[i] = 0xFFFFFFFF;
    }

    /* 使能全局中断 */
    __set_PRIMASK(0);

    /* 跳转到系统BootLoader，首地址是MSP，地址+4是复位中断服务程序地址 */
    SysMemBootJump = (void (*)(void))(*((uint32_t *)(BootAddr + 4)));

    /* 设置主堆栈指针 */
    __set_MSP(*(uint32_t *)BootAddr);

    /* 在RTOS工程，这条语句很重要，设置为特权级模式，使用MSP指针 */
    __set_CONTROL(0);

    /* 跳转到系统BootLoader */
    SysMemBootJump();

    /* 跳转成功的话，不会执行到这里，用户可以在这里添加代码 */
    while (1)
    {
        HAL_Delay(1000);
    }
}

void mem_cfg_show()
{
    const char *pstate_str = "UNKNOWN";
    switch (mem_cfg.pending_state)
    {
    case PSTATE_NONE:
        pstate_str = "NONE";
        break;
    case PSTATE_PENDING:
        pstate_str = "PENDING";
        break;
    case PSTATE_APP_CHECK:
        pstate_str = "APP_CHECK";
        break;
    case PSTATE_APPLIED:
        pstate_str = "APPLIED";
        break;
    case PSTATE_FAILED:
        pstate_str = "FAILED";
        break;
    case PSTATE_BOOT_UP:
        pstate_str = "BOOT_UP";
        break;
    default:
        pstate_str = "UNK";
        break;
    }
    info_printf("\r\n");
    info_printf(" ---------------------------------------\r\n");
    info_printf("| boot :\t0x%08X\t\t|\r\n", mem_cfg.bootloader_address);
    info_printf("| app  :\t0x%08X\t\t|\r\n", mem_cfg.app_address);
    info_printf("| cfg  :\t0x%08X\t\t|\r\n", mem_cfg.cfg_address);
    info_printf("| pend :\t%-9s\t\t|\r\n", pstate_str);
    info_printf(" ---------------------------------------\r\n");
    info_printf("| app  size:0x%08X crc:0x%08X\t|\r\n", mem_cfg.app_hdr.fw_size, mem_cfg.app_hdr.fw_crc);
    info_printf(" ---------------------------------------\r\n");
    info_printf("\r\n");
}
