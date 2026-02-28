#pragma once

#include "stdint.h"

extern void pump_parse(uint8_t *data, uint16_t len);

typedef struct
{
    uint16_t MotorSpeed; // 电机转速 1rpm
    uint8_t state;       // 1-工作 0-停止
    uint8_t PwrLimit;    // 功率限制 1w
} pump_set_param_t;

typedef struct
{
    uint16_t MotorSpeed; // 电机转速 1rpm
    uint8_t state;       // 1-工作 0-停止
    uint16_t DCVoltage;  // 直流母线电压 0.1V
    uint8_t DCCurrent;   // 直流母线电流 0.1V
    int8_t Temp;        // 电机控制器温度 0.5℃ ?
    uint8_t PwrLimit;    // 功率限制 1w
} pump_get_param_t;

extern pump_get_param_t pump_get_param;
extern pump_set_param_t pump_set_param;
extern void pump_setting(pump_set_param_t *p);
extern void pump_ctrl();