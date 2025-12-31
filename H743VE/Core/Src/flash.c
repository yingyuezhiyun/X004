#include "flash.h"

#include "stdio.h"
#include "stdlib.h"
#include "stdint.h"
#include "usart.h"
#include "stm32h7xx_hal_flash.h"

#include "global_cfg.h"

#include <string.h>
#if 0
#include "stm32h750xx.h"
#endif

#define FLASH_PRAGMA_HEAD 0xaaaa
// 偏移128kb
// #define USR_INTER_FLASH_ADDR (FLASH_BANK2_BASE + 0x40000)

// #define USR_INTER_FLASH_ADDR FLASH_BANK2_BASE

#if 1
#if defined(__ICCARM__)
#pragma location = USR_INTER_FLASH_ADDR
const uint8_t para_flash_area[128 * 1024]
#elif defined(__CC_ARM)
const uint8_t para_flash_area[128 * 1024] __attribute__((at(CFG_ADDR)));
#endif
#else
const uint8_t para_flash_area[12];
#endif

#pragma pack(1)
    typedef struct
{
    uint16_t header;   // flash编程头
    uint16_t datasize; // 数据长度
} Flash_info;
#pragma pack()

uint32_t Next_Cfg_Address = CFG_ADDR;
uint32_t Cur_Cfg_Address = CFG_ADDR;

uint32_t GetSector(uint32_t Address)
{
    uint32_t sector = 0;

    if (((Address < ADDR_FLASH_SECTOR_1_BANK1) && (Address >= ADDR_FLASH_SECTOR_0_BANK1)) ||
        ((Address < ADDR_FLASH_SECTOR_1_BANK2) && (Address >= ADDR_FLASH_SECTOR_0_BANK2)))
    {
        sector = FLASH_SECTOR_0;
    }
    else if (((Address < ADDR_FLASH_SECTOR_2_BANK1) && (Address >= ADDR_FLASH_SECTOR_1_BANK1)) ||
             ((Address < ADDR_FLASH_SECTOR_2_BANK2) && (Address >= ADDR_FLASH_SECTOR_1_BANK2)))
    {
        sector = FLASH_SECTOR_1;
    }
    else if (((Address < ADDR_FLASH_SECTOR_3_BANK1) && (Address >= ADDR_FLASH_SECTOR_2_BANK1)) ||
             ((Address < ADDR_FLASH_SECTOR_3_BANK2) && (Address >= ADDR_FLASH_SECTOR_2_BANK2)))
    {
        sector = FLASH_SECTOR_2;
    }
    else if (((Address < ADDR_FLASH_SECTOR_4_BANK1) && (Address >= ADDR_FLASH_SECTOR_3_BANK1)) ||
             ((Address < ADDR_FLASH_SECTOR_4_BANK2) && (Address >= ADDR_FLASH_SECTOR_3_BANK2)))
    {
        sector = FLASH_SECTOR_3;
    }
    else if (((Address < ADDR_FLASH_SECTOR_5_BANK1) && (Address >= ADDR_FLASH_SECTOR_4_BANK1)) ||
             ((Address < ADDR_FLASH_SECTOR_5_BANK2) && (Address >= ADDR_FLASH_SECTOR_4_BANK2)))
    {
        sector = FLASH_SECTOR_4;
    }
    else if (((Address < ADDR_FLASH_SECTOR_6_BANK1) && (Address >= ADDR_FLASH_SECTOR_5_BANK1)) ||
             ((Address < ADDR_FLASH_SECTOR_6_BANK2) && (Address >= ADDR_FLASH_SECTOR_5_BANK2)))
    {
        sector = FLASH_SECTOR_5;
    }
    else if (((Address < ADDR_FLASH_SECTOR_7_BANK1) && (Address >= ADDR_FLASH_SECTOR_6_BANK1)) ||
             ((Address < ADDR_FLASH_SECTOR_7_BANK2) && (Address >= ADDR_FLASH_SECTOR_6_BANK2)))
    {
        sector = FLASH_SECTOR_6;
    }
    else if (((Address < ADDR_FLASH_SECTOR_0_BANK2) && (Address >= ADDR_FLASH_SECTOR_7_BANK1)) ||
             ((Address >= ADDR_FLASH_SECTOR_7_BANK2)))
    {
        sector = FLASH_SECTOR_7;
    }
    else
    {
        sector = FLASH_SECTOR_7;
    }
    return sector;
}

uint8_t IsSectorStart(uint32_t Address)
{
    uint8_t rslt = 0;
    switch (Address)
    {
    case ADDR_FLASH_SECTOR_0_BANK1:
    case ADDR_FLASH_SECTOR_1_BANK1:
    case ADDR_FLASH_SECTOR_2_BANK1:
    case ADDR_FLASH_SECTOR_3_BANK1:
    case ADDR_FLASH_SECTOR_4_BANK1:
    case ADDR_FLASH_SECTOR_5_BANK1:
    case ADDR_FLASH_SECTOR_6_BANK1:
    case ADDR_FLASH_SECTOR_7_BANK1:
    case ADDR_FLASH_SECTOR_0_BANK2:
    case ADDR_FLASH_SECTOR_1_BANK2:
    case ADDR_FLASH_SECTOR_2_BANK2:
    case ADDR_FLASH_SECTOR_3_BANK2:
    case ADDR_FLASH_SECTOR_4_BANK2:
    case ADDR_FLASH_SECTOR_5_BANK2:
    case ADDR_FLASH_SECTOR_6_BANK2:
    case ADDR_FLASH_SECTOR_7_BANK2:
        rslt = 1;
        break;
    default:
        break;
    }
    return rslt;
}

HAL_StatusTypeDef FLASH_Erase(uint32_t _ulFlashAddr)
{

    // 2.擦除FALSH
    // 初始化FLASH_EraseInitTypeDef
    FLASH_EraseInitTypeDef FLASH_Init;
    FLASH_Init.TypeErase = FLASH_TYPEERASE_SECTORS;
    FLASH_Init.Sector = GetSector(_ulFlashAddr);
    FLASH_Init.NbSectors = 1;
    if (_ulFlashAddr >= ADDR_FLASH_SECTOR_0_BANK2)
    {
        FLASH_Init.Banks = FLASH_BANK_2;
    }
    else
    {
        FLASH_Init.Banks = FLASH_BANK_1;
    }
    uint32_t PageError = 0;
    // 3.调用擦除函数
    return HAL_FLASHEx_Erase(&FLASH_Init, &PageError);
}



int FLASH_Write(uint32_t Addr, uint8_t *Data, uint16_t Size)
{
    __set_PRIMASK(1);
    int reslt = 0;
    /*内部FLASH测试*/
    // 1.解锁FLASH
    if (HAL_FLASH_Unlock() != HAL_OK)
    // if(HAL_FLASHEx_Unlock_Bank2()!= HAL_OK)
    {
         __set_PRIMASK(0);
        reslt = -1;
        goto End;
    }
    if (IsSectorStart(Addr))
    {
        if (FLASH_Erase(Addr) != HAL_OK)
        {
            HAL_FLASH_Lock(); 
            __set_PRIMASK(0);
            reslt = -1;
            goto End;
        }
    }

    int data_pos = 0;
    uint32_t flashdata_temp[8];
    while (Size > data_pos)
    {
        memset((char *)flashdata_temp, 0, sizeof(flashdata_temp));
        memcpy((char *)flashdata_temp, Data + data_pos, fmin(32, Size - data_pos));
        if (HAL_FLASH_Program(FLASH_TYPEPROGRAM_FLASHWORD, (Addr + data_pos), flashdata_temp) != HAL_OK)
        {
            reslt = -2;
            goto End;
        }
        data_pos += 32;
    }
End:
    // 5.锁住FLASH
    if (HAL_FLASH_Lock() != HAL_OK)
    // if (HAL_FLASHEx_Lock_Bank2() != HAL_OK)
    {
        __set_PRIMASK(0);
        reslt = -3;
    }
    __set_PRIMASK(0);
    return reslt;
}


void FLASH_ReadData(uint32_t ReadAddr, uint8_t *data, uint32_t Size)
{
    uint32_t i;

    /* 长度为0时不继续操作,否则起始地址为奇地址会出错 */
    if (Size == 0)
    {
        return;
    }
    for (i = 0; i < Size; i++)
    {
        *data++ = *(uint8_t *)ReadAddr++;
    }
}





uint32_t FLASH_ReadWord(uint32_t faddr)
{
    return *(__IO uint32_t *)faddr;
}

int8_t read_data(uint32_t ReadAddr, uint8_t *data, uint32_t Size)
{

    uint32_t temp = FLASH_ReadWord(ReadAddr);
    Flash_info *flash_info;
    flash_info = &temp;
    if (flash_info->header != FLASH_PRAGMA_HEAD) //
    {
        return -1;
    }
    else if (flash_info->datasize != Size)
    {
        return -2;
    }
    FLASH_ReadData(ReadAddr + sizeof(Flash_info), data, Size);
    return 0;
}

int8_t read_data_non_check_size(uint32_t ReadAddr, uint8_t *data, uint32_t Size)
{
    uint32_t temp = FLASH_ReadWord(ReadAddr);
    Flash_info *flash_info;
    flash_info = &temp;
    if (flash_info->header != FLASH_PRAGMA_HEAD) //
    {
        return -1;
    }
    FLASH_ReadData(ReadAddr + sizeof(Flash_info), data, Size);
    return 0;
}

uint16_t read_all_data(uint32_t ReadAddr, uint8_t *data)
{

    uint32_t temp = FLASH_ReadWord(ReadAddr);
    Flash_info *flash_info;
    flash_info = &temp;
    if (flash_info == NULL || flash_info->header != FLASH_PRAGMA_HEAD) //
    {
        return 0;
    }
    FLASH_ReadData(ReadAddr + sizeof(Flash_info), data, flash_info->datasize);
    return flash_info->datasize;
}

void Flash_Ini(uint32_t addr)
{
    uint16_t Next_Index = 0,Cur_Index=0;
    while (1)
    {
        uint32_t temp = FLASH_ReadWord(addr + Next_Index * 32);
        Flash_info *flash_info;
        flash_info = &temp;
        if (flash_info->header == FLASH_PRAGMA_HEAD) //
        {
            Cur_Index=Next_Index;
            Next_Index += ceil((sizeof(Flash_info) + flash_info->datasize) / 32.0);
        }       
        else
        {
            break;
        }
    }
    Cur_Cfg_Address = addr + Cur_Index * 32;
    Next_Cfg_Address = addr + Next_Index * 32;
    if (Next_Cfg_Address >= (addr + sizeof(para_flash_area)))
    {
        Next_Cfg_Address = addr;
    }
}

void read_cfg(void)
{
    Flash_Ini(CFG_ADDR);    
    read_data_non_check_size(Cur_Cfg_Address, &mem_cfg, sizeof(mem_cfg)); 
}

void write_cfg(void)
{
    char buf[sizeof(Flash_info) + sizeof(mem_cfg)];
    Flash_info *head = (Flash_info *)buf;
    head->header = FLASH_PRAGMA_HEAD;
    head->datasize = sizeof(mem_cfg);
    memcpy(buf + sizeof(Flash_info), &mem_cfg, sizeof(mem_cfg));
    if (FLASH_Write(Next_Cfg_Address, (uint8_t *)buf, sizeof(buf)) != 0)
    {
        // 写入失败，重新从头写入
        Cur_Cfg_Address = CFG_ADDR;
        Next_Cfg_Address = CFG_ADDR;
        FLASH_Write(Next_Cfg_Address, (uint8_t *)buf, sizeof(buf));
    }
    Next_Cfg_Address += ceil((double)sizeof(buf) / 32) * 32;
}