#pragma once

#include "stdint.h"
#include "global_cfg.h"
enum
{
    
    TEC_CH_1,
    TEC_CH_2,
    TEC_CH_3,
    TEC_CH_4,
};

extern void SET_TEC_SW(uint8_t ch, uint8_t sw);
extern void TEC_Ctrl(uint8_t tec_ch, tec_setparam_t *stec, tec_measureparam_t *mtec);

