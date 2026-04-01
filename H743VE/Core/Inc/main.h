/* USER CODE BEGIN Header */
/**
 ******************************************************************************
 * @file           : main.h
 * @brief          : Header for main.c file.
 *                   This file contains the common defines of the application.
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
#ifndef __MAIN_H
#define __MAIN_H

#ifdef __cplusplus
extern "C" {
#endif

/* Includes ------------------------------------------------------------------*/
#include "stm32h7xx_hal.h"

/* Private includes ----------------------------------------------------------*/
/* USER CODE BEGIN Includes */

/* USER CODE END Includes */

/* Exported types ------------------------------------------------------------*/
/* USER CODE BEGIN ET */

/* USER CODE END ET */

/* Exported constants --------------------------------------------------------*/
/* USER CODE BEGIN EC */

/* USER CODE END EC */

/* Exported macro ------------------------------------------------------------*/
/* USER CODE BEGIN EM */

/* USER CODE END EM */

/* Exported functions prototypes ---------------------------------------------*/
void Error_Handler(void);

/* USER CODE BEGIN EFP */

/* USER CODE END EFP */

/* Private defines -----------------------------------------------------------*/
#define Temp_CLK_Pin GPIO_PIN_2
#define Temp_CLK_GPIO_Port GPIOE
#define Temp_CS1_Pin GPIO_PIN_3
#define Temp_CS1_GPIO_Port GPIOE
#define Temp_CS2_Pin GPIO_PIN_4
#define Temp_CS2_GPIO_Port GPIOE
#define Temp_MISO_Pin GPIO_PIN_5
#define Temp_MISO_GPIO_Port GPIOE
#define Temp_MOSI_Pin GPIO_PIN_6
#define Temp_MOSI_GPIO_Port GPIOE
#define Temp_CS3_Pin GPIO_PIN_14
#define Temp_CS3_GPIO_Port GPIOC
#define Temp_CS4_Pin GPIO_PIN_15
#define Temp_CS4_GPIO_Port GPIOC
#define TEC1_Cur_Pin GPIO_PIN_0
#define TEC1_Cur_GPIO_Port GPIOC
#define TEC2_Cur_Pin GPIO_PIN_1
#define TEC2_Cur_GPIO_Port GPIOC
#define TEC3_Cur_Pin GPIO_PIN_2
#define TEC3_Cur_GPIO_Port GPIOC
#define TEC4_Cur_Pin GPIO_PIN_3
#define TEC4_Cur_GPIO_Port GPIOC
#define MAX_Cur1_Pin GPIO_PIN_0
#define MAX_Cur1_GPIO_Port GPIOA
#define MAX_Vol1_Pin GPIO_PIN_1
#define MAX_Vol1_GPIO_Port GPIOA
#define MAX_Cur2_Pin GPIO_PIN_2
#define MAX_Cur2_GPIO_Port GPIOA
#define MAX_Vol2_Pin GPIO_PIN_3
#define MAX_Vol2_GPIO_Port GPIOA
#define PCB_TEMP1_Pin GPIO_PIN_4
#define PCB_TEMP1_GPIO_Port GPIOA
#define PCB_TEMP2_Pin GPIO_PIN_5
#define PCB_TEMP2_GPIO_Port GPIOA
#define PCB_TEMP3_Pin GPIO_PIN_6
#define PCB_TEMP3_GPIO_Port GPIOA
#define PCB_TEMP4_Pin GPIO_PIN_7
#define PCB_TEMP4_GPIO_Port GPIOA
#define OPT_PW_Pin GPIO_PIN_4
#define OPT_PW_GPIO_Port GPIOC
#define OPT_TEMP_Pin GPIO_PIN_5
#define OPT_TEMP_GPIO_Port GPIOC
#define ALL_Cur_Pin GPIO_PIN_0
#define ALL_Cur_GPIO_Port GPIOB
#define SYS_Vol_Pin GPIO_PIN_1
#define SYS_Vol_GPIO_Port GPIOB
#define MCU_LOCK_Pin GPIO_PIN_8
#define MCU_LOCK_GPIO_Port GPIOE
#define Q_TRG_OUT_Pin GPIO_PIN_10
#define Q_TRG_OUT_GPIO_Port GPIOB
#define LD_TRG_IN_Pin GPIO_PIN_11
#define LD_TRG_IN_GPIO_Port GPIOB
#define LD_TRG_IN_EXTI_IRQn EXTI15_10_IRQn
#define DAC_CS2_Pin GPIO_PIN_12
#define DAC_CS2_GPIO_Port GPIOB
#define DAC_SPI_CLK_Pin GPIO_PIN_13
#define DAC_SPI_CLK_GPIO_Port GPIOB
#define DAC_CS1_Pin GPIO_PIN_14
#define DAC_CS1_GPIO_Port GPIOB
#define DAC_SPI_MOSI_Pin GPIO_PIN_15
#define DAC_SPI_MOSI_GPIO_Port GPIOB
#define LCVG_ONOFF_Pin GPIO_PIN_15
#define LCVG_ONOFF_GPIO_Port GPIOD
#define FAN_Pin GPIO_PIN_6
#define FAN_GPIO_Port GPIOC
#define LED2_Pin GPIO_PIN_8
#define LED2_GPIO_Port GPIOC
#define LD_TRG_OUT_Pin GPIO_PIN_15
#define LD_TRG_OUT_GPIO_Port GPIOA
#define LD_OC_IN1_Pin GPIO_PIN_0
#define LD_OC_IN1_GPIO_Port GPIOD
#define LD_OC_IN1_EXTI_IRQn EXTI0_IRQn
#define LD_OC_IN2_Pin GPIO_PIN_1
#define LD_OC_IN2_GPIO_Port GPIOD
#define LD_OC_IN2_EXTI_IRQn EXTI1_IRQn
#define LD_ON_OFF2_Pin GPIO_PIN_7
#define LD_ON_OFF2_GPIO_Port GPIOD
#define LD_ON_OFF1_Pin GPIO_PIN_3
#define LD_ON_OFF1_GPIO_Port GPIOB
#define LD_PW_EN2_Pin GPIO_PIN_4
#define LD_PW_EN2_GPIO_Port GPIOB
#define LD_PW_EN1_Pin GPIO_PIN_5
#define LD_PW_EN1_GPIO_Port GPIOB
#define TEC_ON_OFF4_Pin GPIO_PIN_8
#define TEC_ON_OFF4_GPIO_Port GPIOB
#define TEC_ON_OFF3_Pin GPIO_PIN_9
#define TEC_ON_OFF3_GPIO_Port GPIOB
#define TEC_ON_OFF2_Pin GPIO_PIN_0
#define TEC_ON_OFF2_GPIO_Port GPIOE
#define TEC_ON_OFF1_Pin GPIO_PIN_1
#define TEC_ON_OFF1_GPIO_Port GPIOE

/* USER CODE BEGIN Private defines */

// enable ld hardware over current IRQ
#define Enable_LD2_EXIT_DET HAL_NVIC_EnableIRQ(LD_OC_IN2_EXTI_IRQn)

// disable ld hardware over current IRQ
#define Disable_LD2_EXIT_DET HAL_NVIC_DisableIRQ(LD_OC_IN2_EXTI_IRQn)

// enable ld hardware over current IRQ
#define Enable_LD1_EXIT_DET HAL_NVIC_EnableIRQ(LD_OC_IN1_EXTI_IRQn)


// disable ld hardware over current IRQ
#define Disable_LD1_EXIT_DET HAL_NVIC_DisableIRQ(LD_OC_IN1_EXTI_IRQn)


#define Disable_TRG_IN_DET HAL_NVIC_DisableIRQ(LD_TRG_IN_EXTI_IRQn)

#define Enable_TRG_IN_DET HAL_NVIC_EnableIRQ(LD_TRG_IN_EXTI_IRQn)

#define STOP_FAN HAL_GPIO_WritePin(GPIOC, GPIO_PIN_6, GPIO_PIN_SET)

#define START_FAN HAL_GPIO_WritePin(GPIOC, GPIO_PIN_6, GPIO_PIN_RESET)

#define MCU_PW_LOCK HAL_GPIO_WritePin(MCU_LOCK_GPIO_Port, MCU_LOCK_Pin, GPIO_PIN_SET)

/* USER CODE END Private defines */

#ifdef __cplusplus
}
#endif

#endif /* __MAIN_H */
