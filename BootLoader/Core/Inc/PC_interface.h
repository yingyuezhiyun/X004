#pragma once

#include "stdint.h"
#include "global_cfg.h"

void exec_commands_list1(UART_HandleTypeDef *huart, uint8_t cmd, uint8_t *data, size_t data_len);
void exec_commands_list2(UART_HandleTypeDef *huart, uint8_t cmd, uint8_t *data, size_t data_len);

