#ifndef __FLASH_H__
#define __FLASH_H__

#include "stdint.h"

void read_cfg(void);
void write_cfg(void);
int FLASH_Write(uint32_t Addr, uint8_t *Data, uint16_t Size);
// HAL_StatusTypeDef FLASH_Erase(uint32_t _ulFlashAddr);

/* Base address of the Flash sectors Bank 1 */
#ifndef ADDR_FLASH_SECTOR_0_BANK1
#define ADDR_FLASH_SECTOR_0_BANK1 ((uint32_t)0x08000000) /* Base @ of Sector 0, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_1_BANK1
#define ADDR_FLASH_SECTOR_1_BANK1 ((uint32_t)0x08020000) /* Base @ of Sector 1, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_2_BANK1
#define ADDR_FLASH_SECTOR_2_BANK1 ((uint32_t)0x08040000) /* Base @ of Sector 2, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_3_BANK1
#define ADDR_FLASH_SECTOR_3_BANK1 ((uint32_t)0x08060000) /* Base @ of Sector 3, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_4_BANK1
#define ADDR_FLASH_SECTOR_4_BANK1 ((uint32_t)0x08080000) /* Base @ of Sector 4, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_5_BANK1
#define ADDR_FLASH_SECTOR_5_BANK1 ((uint32_t)0x080A0000) /* Base @ of Sector 5, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_6_BANK1
#define ADDR_FLASH_SECTOR_6_BANK1 ((uint32_t)0x080C0000) /* Base @ of Sector 6, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_7_BANK1
#define ADDR_FLASH_SECTOR_7_BANK1 ((uint32_t)0x080E0000) /* Base @ of Sector 7, 128 Kbytes */
#endif

/* Base address of the Flash sectors Bank 2 */
#ifndef ADDR_FLASH_SECTOR_0_BANK2
#define ADDR_FLASH_SECTOR_0_BANK2 ((uint32_t)0x08100000) /* Base @ of Sector 0, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_1_BANK2
#define ADDR_FLASH_SECTOR_1_BANK2 ((uint32_t)0x08120000) /* Base @ of Sector 1, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_2_BANK2
#define ADDR_FLASH_SECTOR_2_BANK2 ((uint32_t)0x08140000) /* Base @ of Sector 2, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_3_BANK2
#define ADDR_FLASH_SECTOR_3_BANK2 ((uint32_t)0x08160000) /* Base @ of Sector 3, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_4_BANK2
#define ADDR_FLASH_SECTOR_4_BANK2 ((uint32_t)0x08180000) /* Base @ of Sector 4, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_5_BANK2
#define ADDR_FLASH_SECTOR_5_BANK2 ((uint32_t)0x081A0000) /* Base @ of Sector 5, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_6_BANK2
#define ADDR_FLASH_SECTOR_6_BANK2 ((uint32_t)0x081C0000) /* Base @ of Sector 6, 128 Kbytes */
#endif
#ifndef ADDR_FLASH_SECTOR_7_BANK2
#define ADDR_FLASH_SECTOR_7_BANK2 ((uint32_t)0x081E0000) /* Base @ of Sector 7, 128 Kbytes */
#endif

#endif
