#pragma once
#include "stdint.h"




void upgrade_ini();
void upgrade_check();


void set_boot_bootmode(uint8_t mode);
uint8_t get_bootmode();
