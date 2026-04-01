#pragma once

#include "stdint.h"
#include "global_cfg.h"
enum
{
    
    LD_CH_1,
    LD_CH_2,
};

typedef struct 
{
    uint8_t flag;
    uint32_t startTickCount;
}status_check_t;

extern void SET_LD_SW(uint8_t ch, uint8_t sw);

extern void LD_Ctrl(uint8_t ld_ch, ld_setparam_t *sld, ld_measureparam_t *mld);

extern void SET_LD_Curr(uint8_t ch, float curr);

extern void SET_LD_Vol(uint8_t ch, float Vol);

extern void SET_LD_MAX_Curr(uint8_t ch, float curr);


extern uint8_t status_deley_check(status_check_t *status_check, uint32_t time_out);

extern void Clear_LD_TRG_IN_Params();

extern void SET_LD_SW_OFF(uint8_t ch);

extern void LD_OC_ADC_Check(uint8_t ch, uint16_t adc_value);

#define LD_MAX_CUR (16.66)


#define LD_MIN_VOL (20.6)
#define LD_MAX_VOL (33.8)

// Helper macros to control LD power and switch (for readability)
// Note: ON_OFF pin is active low in this design (RESET means ON)
#define LD_POWER_ON(ch) ((ch) == LD_CH_1 ? HAL_GPIO_WritePin(LD_PW_EN1_GPIO_Port, LD_PW_EN1_Pin, GPIO_PIN_SET) : HAL_GPIO_WritePin(LD_PW_EN2_GPIO_Port, LD_PW_EN2_Pin, GPIO_PIN_SET))
#define LD_POWER_OFF(ch) ((ch) == LD_CH_1 ? HAL_GPIO_WritePin(LD_PW_EN1_GPIO_Port, LD_PW_EN1_Pin, GPIO_PIN_RESET) : HAL_GPIO_WritePin(LD_PW_EN2_GPIO_Port, LD_PW_EN2_Pin, GPIO_PIN_RESET))
#define LD_SW_ON(ch)  ((ch) == LD_CH_1 ? HAL_GPIO_WritePin(LD_ON_OFF1_GPIO_Port, LD_ON_OFF1_Pin, GPIO_PIN_RESET) : HAL_GPIO_WritePin(LD_ON_OFF2_GPIO_Port, LD_ON_OFF2_Pin, GPIO_PIN_RESET))
#define LD_SW_OFF(ch) ((ch) == LD_CH_1 ? HAL_GPIO_WritePin(LD_ON_OFF1_GPIO_Port, LD_ON_OFF1_Pin, GPIO_PIN_SET)   : HAL_GPIO_WritePin(LD_ON_OFF2_GPIO_Port, LD_ON_OFF2_Pin, GPIO_PIN_SET))

#define IS_LD_SW_ON(ch) ((ch) == LD_CH_1 ? (HAL_GPIO_ReadPin(LD_ON_OFF1_GPIO_Port, LD_ON_OFF1_Pin) == GPIO_PIN_RESET) : (HAL_GPIO_ReadPin(LD_ON_OFF2_GPIO_Port, LD_ON_OFF2_Pin) == GPIO_PIN_RESET))
