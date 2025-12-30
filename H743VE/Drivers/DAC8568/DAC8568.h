#pragma once

#include "stdint.h"

enum
{
    DAC_SEL1,
    DAC_SEL2
};

enum
{
    DAC_CHANNEL_A,
    DAC_CHANNEL_B,
    DAC_CHANNEL_C,
    DAC_CHANNEL_D,
    DAC_CHANNEL_E,
    DAC_CHANNEL_F,
    DAC_CHANNEL_H,
    DAC_CHANNEL_G,
};

void DAC8568_SET(uint8_t sel, uint8_t ch, uint16_t vaule);

#define SET_U_R1_DAC(vaule) DAC8568_SET(DAC_SEL1, DAC_CHANNEL_A, vaule)
#define SET_U_R3_DAC(vaule) DAC8568_SET(DAC_SEL1, DAC_CHANNEL_B, vaule)
#define SET_U_L1_DAC(vaule) DAC8568_SET(DAC_SEL1, DAC_CHANNEL_C, vaule)
#define SET_U_L3_DAC(vaule) DAC8568_SET(DAC_SEL1, DAC_CHANNEL_D, vaule)
#define SET_U_R2_DAC(vaule) DAC8568_SET(DAC_SEL1, DAC_CHANNEL_E, vaule)
#define SET_U_R4_DAC(vaule) DAC8568_SET(DAC_SEL1, DAC_CHANNEL_F, vaule)
#define SET_U_L2_DAC(vaule) DAC8568_SET(DAC_SEL1, DAC_CHANNEL_H, vaule)
#define SET_U_L4_DAC(vaule) DAC8568_SET(DAC_SEL1, DAC_CHANNEL_G, vaule)

#define SET_VOL1_DAC(vaule) DAC8568_SET(DAC_SEL2, DAC_CHANNEL_A, vaule)
#define SET_VOL2_DAC(vaule) DAC8568_SET(DAC_SEL2, DAC_CHANNEL_B, vaule)
#define SET_CUR1_DAC(vaule) DAC8568_SET(DAC_SEL2, DAC_CHANNEL_C, vaule)
#define SET_CUR2_DAC(vaule) DAC8568_SET(DAC_SEL2, DAC_CHANNEL_D, vaule)
#define SET_MAX_CUR1_DAC(vaule) DAC8568_SET(DAC_SEL2, DAC_CHANNEL_E, vaule)
#define SET_MAX_CUR2_DAC(vaule) DAC8568_SET(DAC_SEL2, DAC_CHANNEL_F, vaule)

#define DAC_MAX_VAULE (UINT16_MAX)

#define DAC_REF_V (2.5)
