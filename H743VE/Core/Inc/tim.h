/* USER CODE BEGIN Header */
/**
  ******************************************************************************
  * @file    tim.h
  * @brief   This file contains all the function prototypes for
  *          the tim.c file
  ******************************************************************************
  * @attention
  *
  * Copyright (c) 2025 STMicroelectronics.
  * All rights reserved.
  *
  * This software is licensed under terms that can be found in the LICENSE file
  * in the root directory of this software component.
  * If no LICENSE file comes with this software, it is provided AS-IS.
  *
  ******************************************************************************
  */
/* USER CODE END Header */
/* Define to prevent recursive inclusion -------------------------------------*/
#ifndef __TIM_H__
#define __TIM_H__

#ifdef __cplusplus
extern "C" {
#endif

/* Includes ------------------------------------------------------------------*/
#include "main.h"

/* USER CODE BEGIN Includes */

/* USER CODE END Includes */

extern TIM_HandleTypeDef htim1;

extern TIM_HandleTypeDef htim2;

extern TIM_HandleTypeDef htim3;

extern TIM_HandleTypeDef htim4;

/* USER CODE BEGIN Private defines */

/* USER CODE END Private defines */

void MX_TIM1_Init(void);
void MX_TIM2_Init(void);
void MX_TIM3_Init(void);
void MX_TIM4_Init(void);

void HAL_TIM_MspPostInit(TIM_HandleTypeDef *htim);

/* USER CODE BEGIN Prototypes */


extern void Set_PWM_DutyCycle(TIM_HandleTypeDef *htim, uint32_t __CHANNEL__, uint32_t dutyCycle);
extern void SetPulse_SPWMParam();
extern void SetPulseMode();
extern void SetInterTrgFreq();
extern void CalcPulse_SPWMParam();
extern void CalcPulse_NORParam();

extern void pulse_ini();
extern void pulse_reconfig();

#define SET_FAN_PWM_DutyCycle(x) Set_PWM_DutyCycle(&htim3, TIM_CHANNEL_1, x)
#define START_FAN_PWM HAL_TIM_OC_Start_IT(&htim3, TIM_CHANNEL_1)
#define STOP_FAN_PWM HAL_TIM_OC_Stop_IT(&htim3, TIM_CHANNEL_1)

#define SET_HEAT_PWM_DutyCycle(x) Set_PWM_DutyCycle(&htim1, TIM_CHANNEL_1, x)
#define START_HEAT_PWM HAL_TIM_OC_Start_IT(&htim1, TIM_CHANNEL_1)
#define STOP_HEAT_PWM HAL_TIM_OC_Stop_IT(&htim1, TIM_CHANNEL_1)




/* USER CODE END Prototypes */

#ifdef __cplusplus
}
#endif

#endif /* __TIM_H__ */

