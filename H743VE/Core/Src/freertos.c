/* USER CODE BEGIN Header */
/**
  ******************************************************************************
  * File Name          : freertos.c
  * Description        : Code for freertos applications
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
#include "FreeRTOS.h"
#include "task.h"
#include "main.h"
#include "cmsis_os.h"

/* Private includes ----------------------------------------------------------*/
/* USER CODE BEGIN Includes */
#include "global_cfg.h"
#include "LD_Ctrl.h"
#include "TEC_Ctrl.h"
#include "measure.h"
#include "adc.h"
#include "adc_filt.h"
#include "usart.h"
#include "eeprom.h"
#include "tim.h"
#include "bit_ini.h"
#include "PC_interface.h"
#include "max31865.h"
#include "upgrade.h"
#include "heat.h"
#include "crc.h"
#include "protocol_comm.h"
#include "pump.h"



/* USER CODE END Includes */

/* Private typedef -----------------------------------------------------------*/
/* USER CODE BEGIN PTD */

/* USER CODE END PTD */

/* Private define ------------------------------------------------------------*/
/* USER CODE BEGIN PD */

/* USER CODE END PD */

/* Private macro -------------------------------------------------------------*/
/* USER CODE BEGIN PM */

/* USER CODE END PM */

/* Private variables ---------------------------------------------------------*/
/* USER CODE BEGIN Variables */

/* USER CODE END Variables */
/* Definitions for defaultTask */
osThreadId_t defaultTaskHandle;
const osThreadAttr_t defaultTask_attributes = {
  .name = "defaultTask",
  .stack_size = 128 * 4,
  .priority = (osPriority_t) osPriorityNormal,
};
/* Definitions for myTask02 */
osThreadId_t myTask02Handle;
const osThreadAttr_t myTask02_attributes = {
  .name = "myTask02",
  .stack_size = 128 * 4,
  .priority = (osPriority_t) osPriorityNormal,
};
/* Definitions for myTask03 */
osThreadId_t myTask03Handle;
const osThreadAttr_t myTask03_attributes = {
  .name = "myTask03",
  .stack_size = 128 * 4,
  .priority = (osPriority_t) osPriorityNormal,
};
/* Definitions for myMutex01 */
osMutexId_t myMutex01Handle;
const osMutexAttr_t myMutex01_attributes = {
  .name = "myMutex01"
};
/* Definitions for myMutex02 */
osMutexId_t myMutex02Handle;
const osMutexAttr_t myMutex02_attributes = {
  .name = "myMutex02"
};

/* Private function prototypes -----------------------------------------------*/
/* USER CODE BEGIN FunctionPrototypes */

/* USER CODE END FunctionPrototypes */

void StartDefaultTask(void *argument);
void StartTask02(void *argument);
void StartTask03(void *argument);

void MX_FREERTOS_Init(void); /* (MISRA C 2004 rule 8.1) */

/**
  * @brief  FreeRTOS initialization
  * @param  None
  * @retval None
  */
void MX_FREERTOS_Init(void) {
  /* USER CODE BEGIN Init */

  /* USER CODE END Init */
  /* Create the mutex(es) */
  /* creation of myMutex01 */
  myMutex01Handle = osMutexNew(&myMutex01_attributes);

  /* creation of myMutex02 */
  myMutex02Handle = osMutexNew(&myMutex02_attributes);

  /* USER CODE BEGIN RTOS_MUTEX */
  /* add mutexes, ... */
  /* USER CODE END RTOS_MUTEX */

  /* USER CODE BEGIN RTOS_SEMAPHORES */
  /* add semaphores, ... */
  /* USER CODE END RTOS_SEMAPHORES */

  /* USER CODE BEGIN RTOS_TIMERS */
  /* start timers, add new ones, ... */
  /* USER CODE END RTOS_TIMERS */

  /* USER CODE BEGIN RTOS_QUEUES */
  /* add queues, ... */
  /* USER CODE END RTOS_QUEUES */

  /* Create the thread(s) */
  /* creation of defaultTask */
  defaultTaskHandle = osThreadNew(StartDefaultTask, NULL, &defaultTask_attributes);

  /* creation of myTask02 */
  // myTask02Handle = osThreadNew(StartTask02, NULL, &myTask02_attributes);

  /* creation of myTask03 */
  // myTask03Handle = osThreadNew(StartTask03, NULL, &myTask03_attributes);

  /* USER CODE BEGIN RTOS_THREADS */

  /* add threads, ... */
  /* USER CODE END RTOS_THREADS */

  /* USER CODE BEGIN RTOS_EVENTS */
  /* add events, ... */
  /* USER CODE END RTOS_EVENTS */

}

/* USER CODE BEGIN Header_StartDefaultTask */

void StartCtrlThread()
{
  myTask02Handle = osThreadNew(StartTask02, NULL, &myTask02_attributes);
}
void StartCMDThread()
{
  myTask03Handle = osThreadNew(StartTask03, NULL, &myTask03_attributes);
}
uint32_t test_size = 0;
void crc_test_func()
{
  if (test_size != 0)
  {
    uint32_t _crc = HAL_CRC_Calculate(&hcrc, mem_cfg.app_address, (test_size + 3) / 4);
    info_printf("CRC of APP1 size 0x%X: 0x%08X\r\n", test_size, _crc);
    test_size = 0;
  }
}
/**
  * @brief  Function implementing the defaultTask thread.
  * @param  argument: Not used
  * @retval None
  */
/* USER CODE END Header_StartDefaultTask */
void StartDefaultTask(void *argument)
{
  /* USER CODE BEGIN StartDefaultTask */
  // todo init
  // todo lock
  init_cbuffers();
  Start_ADC_DMA();
  MAX31865_Set_Configuration(MAX31865_Init_2_4_Line);
  MAX31865_Clear_Fault_Status();
  bit_init();
  upgrade_ini();
  Enable_UART1_Receive();
  Enable_UART2_Receive();
  StartCMDThread();


  osThreadExit();
  /* Infinite loop */
  //  for (;;)
  //  {
  //    osDelay(1);
  //  }
  /* USER CODE END StartDefaultTask */
}

/* USER CODE BEGIN Header_StartTask02 */
/**
* @brief Function implementing the myTask02 thread.
* @param argument: Not used
* @retval None
*/
/* USER CODE END Header_StartTask02 */
void StartTask02(void *argument)
{
  /* USER CODE BEGIN StartTask02 */
// osDelay(10);
//  osMutexAcquire(myMutex01Handle, osWaitForever);
//  osMutexRelease(myMutex01Handle);
/* Infinite loop */
#define DEBUGTT
  for (;;)
  {
    get_measure_param();
#ifndef BEBUG_UART

    TEC_Ctrl(TEC_CH_1, &set_param.tec[0], &measure_param.tec[0]);
    TEC_Ctrl(TEC_CH_2, &set_param.tec[1], &measure_param.tec[1]);
    TEC_Ctrl(TEC_CH_3, &set_param.tec[2], &measure_param.tec[2]);
    TEC_Ctrl(TEC_CH_4, &set_param.tec[3], &measure_param.tec[3]);
#ifdef DEBUGTT
    set_param.ld[0].flag = FLAG_DONE;
    set_param.ld[1].flag = FLAG_DONE;
#else
    if (set_param.HardWareTest != 1)
    {
      set_param.ld[0].flag = set_param.tec[0].flag;
      set_param.ld[1].flag = set_param.tec[1].flag;
    }
    else
    {
      set_param.ld[0].flag = FLAG_DONE;
      set_param.ld[1].flag = FLAG_DONE;
    }
#endif // DEBUGTT

    LD_Ctrl(LD_CH_1, &set_param.ld[0], &measure_param.ld[0]);
    LD_Ctrl(LD_CH_2, &set_param.ld[1], &measure_param.ld[1]);


    // Heat_Ctrl();
    // pump_ctrl();
    // crc_test_func();
#endif // BEBUG_UART
#undef DEBUGTT
    osDelay(1);
  }
  /* USER CODE END StartTask02 */
}

/* USER CODE BEGIN Header_StartTask03 */
/**
* @brief Function implementing the myTask03 thread.
* @param argument: Not used
* @retval None
*/
/* USER CODE END Header_StartTask03 */
void StartTask03(void *argument)
{
  /* USER CODE BEGIN StartTask03 */
  //osDelay(10);
  // osMutexAcquire(myMutex02Handle, osWaitForever);
  // osMutexRelease(myMutex02Handle);

  /* Infinite loop */
  for (;;)
  {
    bit_check();
#ifdef BEBUG_UART
    parse_command();
#else
    parse_and_execute_command(&uart2_para, exec_commands_list1);
    parse_and_execute_command(&uart1_para, exec_commands_list2);
    // pc_parse_and_execute_command();
#endif // BEBUG_UART

    osDelay(1);
  }
  /* USER CODE END StartTask03 */
}

/* Private application code --------------------------------------------------*/
/* USER CODE BEGIN Application */

/* USER CODE END Application */

