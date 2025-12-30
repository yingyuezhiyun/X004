#pragma once
#include "stdint.h"

void upgrade(uint8_t *data, uint16_t len);

uint8_t get_upgrade_result();
void upgrade_ini();
void upgrade_check();

#pragma pack(1)
typedef struct
{
    uint16_t cur_idx;
    uint8_t flag;   
} upgrade_status_t;
#pragma pack()
void get_upgrade_status(upgrade_status_t *s);
void delay_swith_boot_mode();
void set_boot_bootmode(uint8_t mode);
uint8_t get_bootmode();