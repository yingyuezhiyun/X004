#pragma once

#include "stdint.h"
#include "global_cfg.h"

void exec_commands(uint8_t cmd, uint8_t *data, size_t data_leng);


#ifdef BEBUG_UART
void parse_command();
#endif // BEBUG_UART
