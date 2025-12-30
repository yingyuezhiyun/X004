#pragma once
#include "stdint.h"

void upgrade(uint8_t *data, uint16_t len);
uint8_t get_upgrade_status();
void upgrade_ini();