/* USER CODE BEGIN Header */
/**
 ******************************************************************************
 * @file    tim.c
 * @brief   This file provides code for the configuration
 *          of the TIM instances.
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
/* Includes ------------------------------------------------------------------*/
#include "tim.h"

/* USER CODE BEGIN 0 */
#include "global_cfg.h"

/* USER CODE END 0 */

TIM_HandleTypeDef htim1;
TIM_HandleTypeDef htim2;
TIM_HandleTypeDef htim3;
TIM_HandleTypeDef htim4;
DMA_HandleTypeDef hdma_tim2_ch1;
DMA_HandleTypeDef hdma_tim2_ch3;

/* TIM1 init function */
void MX_TIM1_Init(void)
{

  /* USER CODE BEGIN TIM1_Init 0 */

  /* USER CODE END TIM1_Init 0 */

  TIM_MasterConfigTypeDef sMasterConfig = {0};
  TIM_OC_InitTypeDef sConfigOC = {0};
  TIM_BreakDeadTimeConfigTypeDef sBreakDeadTimeConfig = {0};

  /* USER CODE BEGIN TIM1_Init 1 */

  /* USER CODE END TIM1_Init 1 */
  htim1.Instance = TIM1;
  htim1.Init.Prescaler = 239;
  htim1.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim1.Init.Period = 65535;
  htim1.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim1.Init.RepetitionCounter = 0;
  htim1.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  if (HAL_TIM_PWM_Init(&htim1) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterOutputTrigger2 = TIM_TRGO2_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim1, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sConfigOC.OCMode = TIM_OCMODE_PWM1;
  sConfigOC.Pulse = 0;
  sConfigOC.OCPolarity = TIM_OCPOLARITY_HIGH;
  sConfigOC.OCNPolarity = TIM_OCNPOLARITY_HIGH;
  sConfigOC.OCFastMode = TIM_OCFAST_DISABLE;
  sConfigOC.OCIdleState = TIM_OCIDLESTATE_RESET;
  sConfigOC.OCNIdleState = TIM_OCNIDLESTATE_RESET;
  if (HAL_TIM_PWM_ConfigChannel(&htim1, &sConfigOC, TIM_CHANNEL_1) != HAL_OK)
  {
    Error_Handler();
  }
  sBreakDeadTimeConfig.OffStateRunMode = TIM_OSSR_DISABLE;
  sBreakDeadTimeConfig.OffStateIDLEMode = TIM_OSSI_DISABLE;
  sBreakDeadTimeConfig.LockLevel = TIM_LOCKLEVEL_OFF;
  sBreakDeadTimeConfig.DeadTime = 0;
  sBreakDeadTimeConfig.BreakState = TIM_BREAK_DISABLE;
  sBreakDeadTimeConfig.BreakPolarity = TIM_BREAKPOLARITY_HIGH;
  sBreakDeadTimeConfig.BreakFilter = 0;
  sBreakDeadTimeConfig.Break2State = TIM_BREAK2_DISABLE;
  sBreakDeadTimeConfig.Break2Polarity = TIM_BREAK2POLARITY_HIGH;
  sBreakDeadTimeConfig.Break2Filter = 0;
  sBreakDeadTimeConfig.AutomaticOutput = TIM_AUTOMATICOUTPUT_DISABLE;
  if (HAL_TIMEx_ConfigBreakDeadTime(&htim1, &sBreakDeadTimeConfig) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM1_Init 2 */

  /* USER CODE END TIM1_Init 2 */
  HAL_TIM_MspPostInit(&htim1);

}
/* TIM2 init function */
void MX_TIM2_Init(void)
{

  /* USER CODE BEGIN TIM2_Init 0 */

  /* USER CODE END TIM2_Init 0 */

  TIM_MasterConfigTypeDef sMasterConfig = {0};
  TIM_OC_InitTypeDef sConfigOC = {0};

  /* USER CODE BEGIN TIM2_Init 1 */

  /* USER CODE END TIM2_Init 1 */
  htim2.Instance = TIM2;
  htim2.Init.Prescaler = 23;
  htim2.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim2.Init.Period = 4294967295;
  htim2.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim2.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  if (HAL_TIM_OC_Init(&htim2) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim2, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sConfigOC.OCMode = TIM_OCMODE_TOGGLE;
  sConfigOC.Pulse = 0;
  sConfigOC.OCPolarity = TIM_OCPOLARITY_LOW;
  sConfigOC.OCFastMode = TIM_OCFAST_DISABLE;
  if (HAL_TIM_OC_ConfigChannel(&htim2, &sConfigOC, TIM_CHANNEL_1) != HAL_OK)
  {
    Error_Handler();
  }
  if (HAL_TIM_OC_ConfigChannel(&htim2, &sConfigOC, TIM_CHANNEL_3) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM2_Init 2 */

  /* USER CODE END TIM2_Init 2 */
  HAL_TIM_MspPostInit(&htim2);

}
/* TIM3 init function */
void MX_TIM3_Init(void)
{

  /* USER CODE BEGIN TIM3_Init 0 */

  /* USER CODE END TIM3_Init 0 */

  TIM_MasterConfigTypeDef sMasterConfig = {0};
  TIM_OC_InitTypeDef sConfigOC = {0};

  /* USER CODE BEGIN TIM3_Init 1 */

  /* USER CODE END TIM3_Init 1 */
  htim3.Instance = TIM3;
  htim3.Init.Prescaler = 239;
  htim3.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim3.Init.Period = 65535;
  htim3.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim3.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  if (HAL_TIM_OC_Init(&htim3) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim3, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sConfigOC.OCMode = TIM_OCMODE_TIMING;
  sConfigOC.Pulse = 0;
  sConfigOC.OCPolarity = TIM_OCPOLARITY_LOW;
  sConfigOC.OCFastMode = TIM_OCFAST_DISABLE;
  if (HAL_TIM_OC_ConfigChannel(&htim3, &sConfigOC, TIM_CHANNEL_1) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM3_Init 2 */
  
  /* USER CODE END TIM3_Init 2 */

}
/* TIM4 init function */
void MX_TIM4_Init(void)
{

  /* USER CODE BEGIN TIM4_Init 0 */

  /* USER CODE END TIM4_Init 0 */

  TIM_MasterConfigTypeDef sMasterConfig = {0};
  TIM_OC_InitTypeDef sConfigOC = {0};

  /* USER CODE BEGIN TIM4_Init 1 */

  /* USER CODE END TIM4_Init 1 */
  htim4.Instance = TIM4;
  htim4.Init.Prescaler = 239;
  htim4.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim4.Init.Period = 65535;
  htim4.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim4.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  if (HAL_TIM_OC_Init(&htim4) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim4, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sConfigOC.OCMode = TIM_OCMODE_TIMING;
  sConfigOC.Pulse = 0;
  sConfigOC.OCPolarity = TIM_OCPOLARITY_HIGH;
  sConfigOC.OCFastMode = TIM_OCFAST_DISABLE;
  if (HAL_TIM_OC_ConfigChannel(&htim4, &sConfigOC, TIM_CHANNEL_1) != HAL_OK)
  {
    Error_Handler();
  }
  if (HAL_TIM_OC_ConfigChannel(&htim4, &sConfigOC, TIM_CHANNEL_2) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM4_Init 2 */

  /* USER CODE END TIM4_Init 2 */

}

void HAL_TIM_PWM_MspInit(TIM_HandleTypeDef* tim_pwmHandle)
{

  if(tim_pwmHandle->Instance==TIM1)
  {
  /* USER CODE BEGIN TIM1_MspInit 0 */

  /* USER CODE END TIM1_MspInit 0 */
    /* TIM1 clock enable */
    __HAL_RCC_TIM1_CLK_ENABLE();
  /* USER CODE BEGIN TIM1_MspInit 1 */

  /* USER CODE END TIM1_MspInit 1 */
  }
}

void HAL_TIM_OC_MspInit(TIM_HandleTypeDef* tim_ocHandle)
{

  if(tim_ocHandle->Instance==TIM2)
  {
  /* USER CODE BEGIN TIM2_MspInit 0 */

  /* USER CODE END TIM2_MspInit 0 */
    /* TIM2 clock enable */
    __HAL_RCC_TIM2_CLK_ENABLE();

    /* TIM2 DMA Init */
    /* TIM2_CH1 Init */
    hdma_tim2_ch1.Instance = DMA2_Stream2;
    hdma_tim2_ch1.Init.Request = DMA_REQUEST_TIM2_CH1;
    hdma_tim2_ch1.Init.Direction = DMA_MEMORY_TO_PERIPH;
    hdma_tim2_ch1.Init.PeriphInc = DMA_PINC_DISABLE;
    hdma_tim2_ch1.Init.MemInc = DMA_MINC_ENABLE;
    hdma_tim2_ch1.Init.PeriphDataAlignment = DMA_PDATAALIGN_WORD;
    hdma_tim2_ch1.Init.MemDataAlignment = DMA_MDATAALIGN_WORD;
    hdma_tim2_ch1.Init.Mode = DMA_NORMAL;
    hdma_tim2_ch1.Init.Priority = DMA_PRIORITY_LOW;
    hdma_tim2_ch1.Init.FIFOMode = DMA_FIFOMODE_DISABLE;
    if (HAL_DMA_Init(&hdma_tim2_ch1) != HAL_OK)
    {
      Error_Handler();
    }

    __HAL_LINKDMA(tim_ocHandle,hdma[TIM_DMA_ID_CC1],hdma_tim2_ch1);

    /* TIM2_CH3 Init */
    hdma_tim2_ch3.Instance = DMA2_Stream3;
    hdma_tim2_ch3.Init.Request = DMA_REQUEST_TIM2_CH3;
    hdma_tim2_ch3.Init.Direction = DMA_MEMORY_TO_PERIPH;
    hdma_tim2_ch3.Init.PeriphInc = DMA_PINC_DISABLE;
    hdma_tim2_ch3.Init.MemInc = DMA_MINC_ENABLE;
    hdma_tim2_ch3.Init.PeriphDataAlignment = DMA_PDATAALIGN_WORD;
    hdma_tim2_ch3.Init.MemDataAlignment = DMA_MDATAALIGN_WORD;
    hdma_tim2_ch3.Init.Mode = DMA_NORMAL;
    hdma_tim2_ch3.Init.Priority = DMA_PRIORITY_LOW;
    hdma_tim2_ch3.Init.FIFOMode = DMA_FIFOMODE_DISABLE;
    if (HAL_DMA_Init(&hdma_tim2_ch3) != HAL_OK)
    {
      Error_Handler();
    }

    __HAL_LINKDMA(tim_ocHandle,hdma[TIM_DMA_ID_CC3],hdma_tim2_ch3);

    /* TIM2 interrupt Init */
    HAL_NVIC_SetPriority(TIM2_IRQn, 5, 0);
    HAL_NVIC_EnableIRQ(TIM2_IRQn);
  /* USER CODE BEGIN TIM2_MspInit 1 */

  /* USER CODE END TIM2_MspInit 1 */
  }
  else if(tim_ocHandle->Instance==TIM3)
  {
  /* USER CODE BEGIN TIM3_MspInit 0 */

  /* USER CODE END TIM3_MspInit 0 */
    /* TIM3 clock enable */
    __HAL_RCC_TIM3_CLK_ENABLE();

    /* TIM3 interrupt Init */
    HAL_NVIC_SetPriority(TIM3_IRQn, 5, 0);
    HAL_NVIC_EnableIRQ(TIM3_IRQn);
  /* USER CODE BEGIN TIM3_MspInit 1 */

  /* USER CODE END TIM3_MspInit 1 */
  }
  else if(tim_ocHandle->Instance==TIM4)
  {
  /* USER CODE BEGIN TIM4_MspInit 0 */

  /* USER CODE END TIM4_MspInit 0 */
    /* TIM4 clock enable */
    __HAL_RCC_TIM4_CLK_ENABLE();

    /* TIM4 interrupt Init */
    HAL_NVIC_SetPriority(TIM4_IRQn, 5, 0);
    HAL_NVIC_EnableIRQ(TIM4_IRQn);
  /* USER CODE BEGIN TIM4_MspInit 1 */

  /* USER CODE END TIM4_MspInit 1 */
  }
}
void HAL_TIM_MspPostInit(TIM_HandleTypeDef* timHandle)
{

  GPIO_InitTypeDef GPIO_InitStruct = {0};
  if(timHandle->Instance==TIM1)
  {
  /* USER CODE BEGIN TIM1_MspPostInit 0 */

  /* USER CODE END TIM1_MspPostInit 0 */
    __HAL_RCC_GPIOE_CLK_ENABLE();
    /**TIM1 GPIO Configuration
    PE9     ------> TIM1_CH1
    */
    GPIO_InitStruct.Pin = GPIO_PIN_9;
    GPIO_InitStruct.Mode = GPIO_MODE_AF_PP;
    GPIO_InitStruct.Pull = GPIO_NOPULL;
    GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
    GPIO_InitStruct.Alternate = GPIO_AF1_TIM1;
    HAL_GPIO_Init(GPIOE, &GPIO_InitStruct);

  /* USER CODE BEGIN TIM1_MspPostInit 1 */

  /* USER CODE END TIM1_MspPostInit 1 */
  }
  else if(timHandle->Instance==TIM2)
  {
  /* USER CODE BEGIN TIM2_MspPostInit 0 */

  /* USER CODE END TIM2_MspPostInit 0 */

    __HAL_RCC_GPIOB_CLK_ENABLE();
    __HAL_RCC_GPIOA_CLK_ENABLE();
    /**TIM2 GPIO Configuration
    PB10     ------> TIM2_CH3
    PA15 (JTDI)     ------> TIM2_CH1
    */
    GPIO_InitStruct.Pin = Q_TRG_OUT_Pin;
    GPIO_InitStruct.Mode = GPIO_MODE_AF_PP;
    GPIO_InitStruct.Pull = GPIO_PULLDOWN;
    GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_HIGH;
    GPIO_InitStruct.Alternate = GPIO_AF1_TIM2;
    HAL_GPIO_Init(Q_TRG_OUT_GPIO_Port, &GPIO_InitStruct);

    GPIO_InitStruct.Pin = LD_TRG_OUT_Pin;
    GPIO_InitStruct.Mode = GPIO_MODE_AF_PP;
    GPIO_InitStruct.Pull = GPIO_PULLDOWN;
    GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_HIGH;
    GPIO_InitStruct.Alternate = GPIO_AF1_TIM2;
    HAL_GPIO_Init(LD_TRG_OUT_GPIO_Port, &GPIO_InitStruct);

  /* USER CODE BEGIN TIM2_MspPostInit 1 */

  /* USER CODE END TIM2_MspPostInit 1 */
  }

}

void HAL_TIM_PWM_MspDeInit(TIM_HandleTypeDef* tim_pwmHandle)
{

  if(tim_pwmHandle->Instance==TIM1)
  {
  /* USER CODE BEGIN TIM1_MspDeInit 0 */

  /* USER CODE END TIM1_MspDeInit 0 */
    /* Peripheral clock disable */
    __HAL_RCC_TIM1_CLK_DISABLE();
  /* USER CODE BEGIN TIM1_MspDeInit 1 */

  /* USER CODE END TIM1_MspDeInit 1 */
  }
}

void HAL_TIM_OC_MspDeInit(TIM_HandleTypeDef* tim_ocHandle)
{

  if(tim_ocHandle->Instance==TIM2)
  {
  /* USER CODE BEGIN TIM2_MspDeInit 0 */

  /* USER CODE END TIM2_MspDeInit 0 */
    /* Peripheral clock disable */
    __HAL_RCC_TIM2_CLK_DISABLE();

    /* TIM2 DMA DeInit */
    HAL_DMA_DeInit(tim_ocHandle->hdma[TIM_DMA_ID_CC1]);
    HAL_DMA_DeInit(tim_ocHandle->hdma[TIM_DMA_ID_CC3]);

    /* TIM2 interrupt Deinit */
    HAL_NVIC_DisableIRQ(TIM2_IRQn);
  /* USER CODE BEGIN TIM2_MspDeInit 1 */

  /* USER CODE END TIM2_MspDeInit 1 */
  }
  else if(tim_ocHandle->Instance==TIM3)
  {
  /* USER CODE BEGIN TIM3_MspDeInit 0 */

  /* USER CODE END TIM3_MspDeInit 0 */
    /* Peripheral clock disable */
    __HAL_RCC_TIM3_CLK_DISABLE();

    /* TIM3 interrupt Deinit */
    HAL_NVIC_DisableIRQ(TIM3_IRQn);
  /* USER CODE BEGIN TIM3_MspDeInit 1 */

  /* USER CODE END TIM3_MspDeInit 1 */
  }
  else if(tim_ocHandle->Instance==TIM4)
  {
  /* USER CODE BEGIN TIM4_MspDeInit 0 */

  /* USER CODE END TIM4_MspDeInit 0 */
    /* Peripheral clock disable */
    __HAL_RCC_TIM4_CLK_DISABLE();

    /* TIM4 interrupt Deinit */
    HAL_NVIC_DisableIRQ(TIM4_IRQn);
  /* USER CODE BEGIN TIM4_MspDeInit 1 */

  /* USER CODE END TIM4_MspDeInit 1 */
  }
}

/* USER CODE BEGIN 1 */

void HAL_TIM_IC_CaptureCallback(TIM_HandleTypeDef *htim)
{
  if (htim->Instance == htim3.Instance)
  {
  }
}

void HAL_TIM_OC_DelayElapsedCallback(TIM_HandleTypeDef *htim)
{

  if (htim->Instance == htim2.Instance)
  {
    if (htim->Channel == HAL_TIM_ACTIVE_CHANNEL_1)
    {
    }
    if (htim->Channel == HAL_TIM_ACTIVE_CHANNEL_2)
    {
    }
  }
  if (htim->Instance == htim4.Instance)
  {
    if (htim->Channel == HAL_TIM_ACTIVE_CHANNEL_1 && set_param.TRG_Type == TRG_INTER &&
        (running_flag.ld_flags[0].content.is_en == 1 || running_flag.ld_flags[1].content.is_en == 1)) // 内部触发
    {
      if (set_param.Pulse_Type == PULSE_SPWM) // 变频
      {
        SetPulse_SPWMParam();
      }
      else if (running_flag.pulse_flags.value == 0) // 定频
      {
        SetInterTrgFreq();
        // SetPulse_NORParam();
        // HAL_TIM_Base_Start_IT(&htim2);
        running_flag.pulse_flags.content.inter_nor = FLAG_RUNNING;
      }
    }

    // if (htim->Channel == HAL_TIM_ACTIVE_CHANNEL_2) // todo 外部触发+变频 判断频率
    // {
    // }
  }
}
void HAL_TIM_PWM_PulseFinishedCallback(TIM_HandleTypeDef *htim)
{
  /* Prevent unused argument(s) compilation warning */
  UNUSED(htim);

  /* NOTE : This function should not be modified, when the callback is needed,
            the HAL_TIM_PWM_PulseFinishedCallback could be implemented in the user file
   */
  if (htim->Instance == htim2.Instance)
  {
    if (htim->Channel == HAL_TIM_ACTIVE_CHANNEL_1) //
    {
      HAL_GPIO_WritePin(LD_TRG_OUT_GPIO_Port, LD_TRG_OUT_Pin, GPIO_PIN_RESET);
      // __HAL_TIM_SET_COMPARE(&htim2, TIM_CHANNEL_1, 4294967295-1);
    }
    if (htim->Channel == HAL_TIM_ACTIVE_CHANNEL_3) //
    {
      HAL_GPIO_WritePin(Q_TRG_OUT_GPIO_Port, Q_TRG_OUT_Pin, GPIO_PIN_RESET);
      // __HAL_TIM_SET_COMPARE(&htim2, TIM_CHANNEL_2, 4294967295-1);
    }
  }
}
void HAL_TIM_ErrorCallback(TIM_HandleTypeDef *htim)
{
  /* Prevent unused argument(s) compilation warning */
  UNUSED(htim);

  /* NOTE : This function should not be modified, when the callback is needed,
            the HAL_TIM_ErrorCallback could be implemented in the user file
   */
  if (htim->Instance == htim2.Instance)
  {
    if (htim->Channel == HAL_TIM_ACTIVE_CHANNEL_1) //
    {
    }
  }
}

/// @brief 设置占空比
/// @param htim
/// @param __CHANNEL__
/// @param dutyCycle
void Set_PWM_DutyCycle(TIM_HandleTypeDef *htim, uint32_t __CHANNEL__, uint32_t dutyCycle)
{
  // 假设定时器周期为 1000
  uint32_t compareValue = (htim->Init.Period + 1) * dutyCycle / 100;
  __HAL_TIM_SET_COMPARE(htim, __CHANNEL__, compareValue);
}

uint32_t LD_TRG_RCC_SPWM[24];
uint16_t LD_TRG_RCC_SPWM_Len = 0;
uint32_t Q_TRG_RCC_SPWM[24];
uint16_t Q_TRG_RCC_SPWM_Len = 0;
uint32_t LD_TRG_RCC_NOR[2];
uint32_t Q_TRG_RCC_NOR[2];
#define TIM_PULSE_STEP_US (10)
#define TIM2_CLK (10000000.0)
// #define QWidth_us (100)
#define QWidth_us (5)
#define PreCNT (50)
#define Pulse_Tim_Delay_us (10)

void CalcPulse_SPWMParam()
{
  uint16_t Pwidth = set_param.Pulse_para.Width;

  uint16_t Qdelay = set_param.T_Q.delay;
  uint8_t idx = 0;
  uint32_t basetick = PreCNT * TIM_PULSE_STEP_US;
#if 0
  LD_TRG_RCC_SPWM[idx++] = basetick;
  basetick += Pwidth * TIM_PULSE_STEP_US;
  LD_TRG_RCC_SPWM[idx++] = basetick;
  for (size_t i = 0; i < set_param.Pulse_para.Num; i++)
  {
    basetick += (set_param.Pulse_para.Interval[i] - Pwidth) * TIM_PULSE_STEP_US;
    LD_TRG_RCC_SPWM[idx++] = basetick;
    basetick += Pwidth * TIM_PULSE_STEP_US;
    LD_TRG_RCC_SPWM[idx++] = basetick;
  }
#else // 脉宽等于间距的情况
  LD_TRG_RCC_SPWM[idx++] = basetick;
  for (size_t i = 0; i < set_param.Pulse_para.Num; i++)
  {
    while (set_param.Pulse_para.Interval[i] <= Pwidth)
    {
      basetick += set_param.Pulse_para.Interval[i];
      i++;
    }
    basetick += Pwidth * TIM_PULSE_STEP_US;
    LD_TRG_RCC_SPWM[idx++] = basetick;

    basetick += (set_param.Pulse_para.Interval[i] - Pwidth) * TIM_PULSE_STEP_US;
    LD_TRG_RCC_SPWM[idx++] = basetick;
  }
  basetick += Pwidth * TIM_PULSE_STEP_US;
  LD_TRG_RCC_SPWM[idx++] = basetick;
#endif
  LD_TRG_RCC_SPWM_Len = idx;

  idx = 0;
  basetick = PreCNT * TIM_PULSE_STEP_US + Qdelay * TIM_PULSE_STEP_US;
  Q_TRG_RCC_SPWM[idx++] = basetick;
  basetick += QWidth_us * TIM_PULSE_STEP_US;
  Q_TRG_RCC_SPWM[idx++] = basetick;
  for (size_t i = 0; i < set_param.Pulse_para.Num; i++)
  {
    basetick += (set_param.Pulse_para.Interval[i] - QWidth_us) * TIM_PULSE_STEP_US;
    Q_TRG_RCC_SPWM[idx++] = basetick;
    basetick += QWidth_us * TIM_PULSE_STEP_US;
    Q_TRG_RCC_SPWM[idx++] = basetick;
  }
  Q_TRG_RCC_SPWM_Len = idx;
}

void CalcPulse_NORParam()
{
  double pt = TIM2_CLK / set_param.Pulse_para.Nor_Freq / 1000;

  LD_TRG_RCC_NOR[0] = PreCNT * TIM_PULSE_STEP_US;
  LD_TRG_RCC_NOR[1] = LD_TRG_RCC_NOR[0] + set_param.Pulse_para.Width * pt;
  Q_TRG_RCC_NOR[0] = PreCNT * TIM_PULSE_STEP_US + set_param.T_Q.delay * pt;
  Q_TRG_RCC_NOR[1] = Q_TRG_RCC_NOR[0] + QWidth_us * pt;
}

void SetPulse_SPWMParam()
{
  if (running_flag.pulse_flags.value != 0)
  {
    return;
  }

  // __HAL_TIM_SET_COUNTER(&htim7, 0);
  // __HAL_TIM_SET_PRESCALER(&htim7, prescaler);
  // __HAL_TIM_SET_AUTORELOAD(&htim7, period);
  //__HAL_TIM_SET_CLOCKDIVISION
  //__HAL_TIM_SET_ICPRESCALER
  //__HAL_TIM_SET_COMPARE
  // HAL_TIM_OC_Start_DMA

  // HAL_TIM_OC_Stop_DMA(&htim2, TIM_CHANNEL_3);
  // HAL_TIM_OC_Stop_DMA(&htim2, TIM_CHANNEL_1);

  // HAL_TIM_Base_Stop(&htim2);
  // __HAL_TIM_SET_COUNTER(&htim2, 0);

  // HAL_GPIO_WritePin(LD_TRG_OUT_GPIO_Port, LD_TRG_OUT_Pin, GPIO_PIN_RESET);
  // HAL_GPIO_WritePin(Q_TRG_OUT_GPIO_Port, Q_TRG_OUT_Pin, GPIO_PIN_RESET);
  HAL_TIM_OC_DMA_Config(&htim2, TIM_CHANNEL_1, LD_TRG_RCC_SPWM, LD_TRG_RCC_SPWM_Len);
  if (set_param.T_Q.sw == WORK_ON)
  {
    HAL_TIM_OC_DMA_Config(&htim2, TIM_CHANNEL_3, Q_TRG_RCC_SPWM, Q_TRG_RCC_SPWM_Len);
    uint32_t MAX_P = LD_TRG_RCC_SPWM[LD_TRG_RCC_SPWM_Len - 1] > Q_TRG_RCC_SPWM[Q_TRG_RCC_SPWM_Len - 1] ? LD_TRG_RCC_SPWM[LD_TRG_RCC_SPWM_Len - 1] : Q_TRG_RCC_SPWM[Q_TRG_RCC_SPWM_Len - 1];
    __HAL_TIM_SET_AUTORELOAD(&htim2, MAX_P + Pulse_Tim_Delay_us * TIM_PULSE_STEP_US - 1);
  }
  else
  {
    TIM_CCxChannelCmd(htim2.Instance, TIM_CHANNEL_3, TIM_CCx_DISABLE);
    __HAL_TIM_SET_AUTORELOAD(&htim2, LD_TRG_RCC_SPWM[LD_TRG_RCC_SPWM_Len - 1] + Pulse_Tim_Delay_us * TIM_PULSE_STEP_US - 1);
  }
  HAL_TIM_Base_Start_IT(&htim2);

  if (set_param.TRG_Type == TRG_INTER)
  {
    running_flag.pulse_flags.content.inter_spwm = FLAG_RUNNING;
  }
  else
  {
    running_flag.pulse_flags.content.out_spwm = FLAG_RUNNING;
  }

  // HAL_TIM_OC_Start_DMA
}

void SetPulse_NORParam()
{
  // HAL_GPIO_WritePin(LD_TRG_OUT_GPIO_Port, LD_TRG_OUT_Pin, GPIO_PIN_RESET);
  // HAL_GPIO_WritePin(Q_TRG_OUT_GPIO_Port, Q_TRG_OUT_Pin, GPIO_PIN_RESET);
  HAL_TIM_OC_DMA_Config(&htim2, TIM_CHANNEL_1, LD_TRG_RCC_NOR, 2);
  HAL_TIM_OC_DMA_Config(&htim2, TIM_CHANNEL_3, Q_TRG_RCC_NOR, 2);
}

void SetInterTrgFreq()
{
  HAL_TIM_OC_Stop(&htim2, TIM_CHANNEL_1);
  HAL_TIM_OC_Stop(&htim2, TIM_CHANNEL_3);
  HAL_TIM_Base_Stop_IT(&htim2);
  __HAL_TIM_SET_COUNTER(&htim2, 0);
  uint32_t period = TIM2_CLK / set_param.Pulse_para.Nor_Freq;
  __HAL_TIM_SET_AUTORELOAD(&htim2, period - 1);
  CalcPulse_NORParam();
  SetPulse_NORParam();
  HAL_TIM_Base_Start_IT(&htim2);
}

void pulse_ini()
{
  running_flag.pulse_flags.value = 0;
  __HAL_TIM_SET_COMPARE(&htim4, TIM_CHANNEL_1, 100);
  __HAL_TIM_SET_AUTORELOAD(&htim4, 20000 - 1);
  HAL_TIM_OC_Start_IT(&htim4, TIM_CHANNEL_1);
  HAL_TIM_Base_Start(&htim4);
}

void pulse_reconfig()
{
  if (set_param.TRG_Type == TRG_INTER && set_param.Pulse_Type == PULSE_NOR &&
      (running_flag.ld_flags[0].content.is_en == 1 || running_flag.ld_flags[1].content.is_en == 1)) // 内部定频
  {
    running_flag.pulse_flags.content.inter_nor = FLAG_RUNNING;
    SetPulse_NORParam();
  }
  else
  {
    running_flag.pulse_flags.content.inter_nor = FLAG_IDLE;
    HAL_TIM_OC_Stop(&htim2, TIM_CHANNEL_1);
    HAL_TIM_OC_Stop(&htim2, TIM_CHANNEL_3);
    HAL_TIM_Base_Stop_IT(&htim2);
    __HAL_TIM_SET_COUNTER(&htim2, 0);
  }

  if (set_param.Pulse_Type == PULSE_SPWM) // 变频
  {
    // HAL_TIM_Base_Stop(&htim2);
    HAL_TIM_OC_Stop(&htim2, TIM_CHANNEL_1);
    HAL_TIM_OC_Stop(&htim2, TIM_CHANNEL_3);
    HAL_TIM_Base_Stop_IT(&htim2);

    running_flag.pulse_flags.value = 0;
    __HAL_TIM_SET_COUNTER(&htim2, 0);
  }
}

#if 0
void HAL_TIM2_PWM_MspPostInit(TIM_HandleTypeDef *timHandle)
{
  GPIO_InitTypeDef GPIO_InitStruct = {0};
  /* USER CODE BEGIN TIM2_MspPostInit 0 */

  /* USER CODE END TIM2_MspPostInit 0 */

  __HAL_RCC_GPIOB_CLK_ENABLE();
  __HAL_RCC_GPIOA_CLK_ENABLE();
  /**TIM2 GPIO Configuration
  PB10     ------> TIM2_CH3
  PA15 (JTDI)     ------> TIM2_CH1
  */
  GPIO_InitStruct.Pin = Q_TRG_OUT_Pin;
  GPIO_InitStruct.Mode = GPIO_MODE_AF_PP;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  GPIO_InitStruct.Alternate = GPIO_AF1_TIM2;
  HAL_GPIO_Init(Q_TRG_OUT_GPIO_Port, &GPIO_InitStruct);

  GPIO_InitStruct.Pin = LD_TRG_OUT_Pin;
  GPIO_InitStruct.Mode = GPIO_MODE_AF_PP;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  GPIO_InitStruct.Alternate = GPIO_AF1_TIM2;
  HAL_GPIO_Init(LD_TRG_OUT_GPIO_Port, &GPIO_InitStruct);

  /* USER CODE BEGIN TIM2_MspPostInit 1 */

  /* USER CODE END TIM2_MspPostInit 1 */
}

void MX_TIM2_PWM_Init(void)
{
  /* USER CODE BEGIN TIM2_Init 0 */

  /* USER CODE END TIM2_Init 0 */

  TIM_MasterConfigTypeDef sMasterConfig = {0};
  TIM_OC_InitTypeDef sConfigOC = {0};

  /* USER CODE BEGIN TIM2_Init 1 */

  /* USER CODE END TIM2_Init 1 */
  htim2.Instance = TIM2;
  htim2.Init.Prescaler = 23;
  htim2.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim2.Init.Period = 4294967295;
  htim2.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim2.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  if (HAL_TIM_PWM_Init(&htim2) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim2, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sConfigOC.OCMode = TIM_OCMODE_PWM1;
  sConfigOC.Pulse = 0;
  sConfigOC.OCPolarity = TIM_OCPOLARITY_LOW;
  sConfigOC.OCFastMode = TIM_OCFAST_DISABLE;
  if (HAL_TIM_PWM_ConfigChannel(&htim2, &sConfigOC, TIM_CHANNEL_1) != HAL_OK)
  {
    Error_Handler();
  }
  if (HAL_TIM_PWM_ConfigChannel(&htim2, &sConfigOC, TIM_CHANNEL_3) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM2_Init 2 */

  /* USER CODE END TIM2_Init 2 */
  HAL_TIM2_PWM_MspPostInit(&htim2);
}

void SetPulseMode()
{
  HAL_TIM_OC_Stop_IT(&htim4, TIM_CHANNEL_1);
  HAL_TIM_Base_Stop(&htim4);

  Delay_ms(10);
  HAL_GPIO_WritePin(LD_TRG_OUT_GPIO_Port, LD_TRG_OUT_Pin, GPIO_PIN_RESET);
  HAL_GPIO_WritePin(Q_TRG_OUT_GPIO_Port, Q_TRG_OUT_Pin, GPIO_PIN_RESET);

  // HAL_GPIO_WritePin(LD_TRG_OUT_GPIO_Port, LD_TRG_OUT_Pin, GPIO_PIN_SET);
  // HAL_GPIO_WritePin(Q_TRG_OUT_GPIO_Port, Q_TRG_OUT_Pin, GPIO_PIN_SET);

  // HAL_TIM_OC_Stop_DMA(&htim2, TIM_CHANNEL_3);
  // HAL_TIM_OC_Stop_DMA(&htim2, TIM_CHANNEL_1);
  HAL_TIM_Base_Stop(&htim2);
  __HAL_TIM_CLEAR_FLAG(&htim2, TIM_FLAG_UPDATE);
  __HAL_RCC_TIM2_CLK_DISABLE();
  HAL_DMA_DeInit(htim2.hdma[TIM_DMA_ID_CC3]);
  HAL_DMA_DeInit(htim2.hdma[TIM_DMA_ID_CC1]);
  HAL_NVIC_DisableIRQ(TIM2_IRQn);
  htim2.State = HAL_TIM_STATE_RESET;
  Delay_ms(10);
  HAL_GPIO_WritePin(LD_TRG_OUT_GPIO_Port, LD_TRG_OUT_Pin, GPIO_PIN_RESET);
  HAL_GPIO_WritePin(Q_TRG_OUT_GPIO_Port, Q_TRG_OUT_Pin, GPIO_PIN_RESET);
  MX_TIM2_Init();
  // todo
  HAL_TIM_Base_Stop(&htim2);

  __HAL_TIM_SET_COUNTER(&htim2, 0);
  __HAL_TIM_SET_COUNTER(&htim4, 0);
  if (set_param.TRG_Type == TRG_INTER && set_param.Pulse_Type == PULSE_SPWM) // 内部变频
  {

    __HAL_TIM_SET_AUTORELOAD(&htim2, 20000 * TIM_PULSE_STEP_US); // todo
    __HAL_TIM_SET_COMPARE(&htim4, TIM_CHANNEL_1, 50);
    __HAL_TIM_SET_AUTORELOAD(&htim4, 20000 - 1);
    HAL_TIM_OC_Start_IT(&htim4, TIM_CHANNEL_1);
    // HAL_TIM_OC_Stop(&htim4, TIM_CHANNEL_2);
    HAL_TIM_Base_Start(&htim4);
  }
  else if (set_param.TRG_Type == TRG_INTER && set_param.Pulse_Type == PULSE_NOR) // 内部定频
  {
    SetInterTrgFreq();
  }
  else // 外部
  {

    __HAL_TIM_SET_AUTORELOAD(&htim2, 20000*TIM_PULSE_STEP_US);//todo
    __HAL_TIM_SET_COUNTER(&htim4, 0);
    __HAL_TIM_SET_AUTORELOAD(&htim4, 65535);
    // HAL_TIM_OC_Start(&htim4, TIM_CHANNEL_2);
    Clear_LD_TRG_IN_Params();
    HAL_TIM_Base_Start(&htim4);
  }

#if 0
  if (set_param.TRG_Type == TRG_INTER && set_param.Pulse_Type == PULSE_SPWM) // 内部变频
  {
    HAL_TIM_Base_Stop(&htim4);
    __HAL_RCC_TIM2_CLK_DISABLE();
    MX_TIM2_Init();

   
    __HAL_TIM_SET_COUNTER(&htim4, 0);
    __HAL_TIM_SET_COMPARE(&htim4, TIM_CHANNEL_1, 50);
    __HAL_TIM_SET_AUTORELOAD(&htim4, 20000 - 1);
    HAL_TIM_OC_Start(&htim4, TIM_CHANNEL_1);
    HAL_TIM_OC_Stop(&htim4, TIM_CHANNEL_2);
    HAL_TIM_Base_Start(&htim4);
  }
  else if (set_param.TRG_Type == TRG_INTER && set_param.Pulse_Type == PULSE_NOR) // 内部定频
  {
    HAL_TIM_Base_Stop(&htim4);

    /* Peripheral clock disable */
    __HAL_RCC_TIM2_CLK_DISABLE();
    /* TIM4 DMA DeInit */
    HAL_DMA_DeInit(htim2.hdma[TIM_DMA_ID_CC3]);
    HAL_DMA_DeInit(htim2.hdma[TIM_DMA_ID_CC1]);
    MX_TIM2_PWM_Init();

    SetInterTrgFreq();
  }
  else // 外部
  {
    HAL_TIM_Base_Stop(&htim4);
    __HAL_RCC_TIM2_CLK_DISABLE();
    MX_TIM2_Init();

   
    __HAL_TIM_SET_COUNTER(&htim4, 0);
    __HAL_TIM_SET_COMPARE(&htim4, TIM_CHANNEL_1, 50);
    __HAL_TIM_SET_AUTORELOAD(&htim4, 20000 - 1);
    HAL_TIM_OC_Start(&htim4, TIM_CHANNEL_1);
    HAL_TIM_OC_Stop(&htim4, TIM_CHANNEL_2);
    HAL_TIM_Base_Start(&htim4);
  }
#endif
}
#endif

/* USER CODE END 1 */
