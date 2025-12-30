#pragma once

#include "stdint.h"
#include "global_cfg.h"

void pc_parse_and_execute_command2();


#ifdef BEBUG_UART
void parse_command();
#endif // BEBUG_UART
