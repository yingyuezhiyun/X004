/* USER CODE BEGIN Header */
/**
  ******************************************************************************
  * @file           : main.c
  * @brief          : Main program body
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
#include "main.h"
#include "memorymap.h"
#include "usart.h"
#include "gpio.h"

/* Private includes ----------------------------------------------------------*/
/* USER CODE BEGIN Includes */
#include "stdio.h"
#include "string.h"
#include "global_cfg.h"
#include "flash.h"
#include "stdarg.h"
/* USER CODE END Includes */

/* Private typedef -----------------------------------------------------------*/
/* USER CODE BEGIN PTD */

/* USER CODE END PTD */

/* Private define ------------------------------------------------------------*/
/* USER CODE BEGIN PD */



/* USER CODE END PD */

/* Private macro -------------------------------------------------------------*/
/* USER CODE BEGIN PM */
// #ifdef __GNUC__
//   #define PUTCHAR_PROTOTYPE int __io_putchar(int ch)
// #else
//   #define PUTCHAR_PROTOTYPE int fputc(int ch, FILE *f)
// #endif

// PUTCHAR_PROTOTYPE
// {
//   HAL_UART_Transmit(&huart1,(uint8_t *)&ch,1,0xFFFF);   //阻塞方式打印,串口1
//   return ch;
// }

void uart1_printf(char *format, ...)
{
  char UartTxBuff[256];
  va_list args;
  uint32_t length;
  va_start(args, format);
  length = vsnprintf((char *)UartTxBuff, 256, (char *)format, args);
  va_end(args);
  HAL_UART_Transmit(&huart1, UartTxBuff, length, 0xfff);

  // HAL_UART_Transmit_DMA(huart, UartTxBuff, length);
}

void info_printf(char *format, ...)
{
  if (INFO_PRINT)
  {
    char UartTxBuff[256];
    va_list args;
    uint32_t length;
    va_start(args, format);
    length = vsnprintf((char *)UartTxBuff, 256, (char *)format, args);
    va_end(args);
   // HAL_UART_Transmit(&huart2, UartTxBuff, length, 0xfff);
    HAL_UART_Transmit(&huart1, UartTxBuff, length, 0xfff);
    // HAL_UART_Transmit_DMA(huart, UartTxBuff, length);
  }
}

/* USER CODE END PM */

/* Private variables ---------------------------------------------------------*/

/* USER CODE BEGIN PV */
mem_cfg_t mem_cfg = {

    .bootloader_address = BL_ADDR,
    .app1_address = APP1_ADDR,
    .app2_address = APP2_ADDR,
    .cfg_address = CFG_ADDR,
};

/* USER CODE END PV */

/* Private function prototypes -----------------------------------------------*/
void SystemClock_Config(void);
static void MPU_Config(void);
/* USER CODE BEGIN PFP */
void Jump_to_APP2();
/* USER CODE END PFP */

/* Private user code ---------------------------------------------------------*/
/* USER CODE BEGIN 0 */

/* USER CODE END 0 */

/**
  * @brief  The application entry point.
  * @retval int
  */
int main(void)
{

  /* USER CODE BEGIN 1 */

  /* USER CODE END 1 */

  /* MPU Configuration--------------------------------------------------------*/
  MPU_Config();

  /* MCU Configuration--------------------------------------------------------*/

  /* Reset of all peripherals, Initializes the Flash interface and the Systick. */
  HAL_Init();

  /* USER CODE BEGIN Init */

  /* USER CODE END Init */

  /* Configure the system clock */
  SystemClock_Config();

  /* USER CODE BEGIN SysInit */

  /* USER CODE END SysInit */

  /* Initialize all configured peripherals */
  MX_GPIO_Init();
  MX_USART1_UART_Init();
  MX_USART2_UART_Init();
  /* USER CODE BEGIN 2 */
  info_printf("BootLoader Running...\r\n");
  info_printf("Read Config...\r\n");
  read_cfg();
  info_printf("\r\n");
  info_printf("--------------------------------------\r\n");
  info_printf("| boot :\t0x%08X           |\r\n", mem_cfg.bootloader_address);
  info_printf("| app1 :\t0x%08X           |\r\n", mem_cfg.app1_address);
  info_printf("| app2 :\t0x%08X           |\r\n", mem_cfg.app2_address);
  info_printf("| curr :\t0x%08X           |\r\n", mem_cfg.current_app_address);
  info_printf("| cfg  :\t0x%08X           |\r\n", mem_cfg.cfg_address);
  info_printf("--------------------------------------\r\n");
  info_printf("\r\n");
  info_printf("Check App...\r\n");
  Jump_to_APP2();

  /* USER CODE END 2 */

  /* Infinite loop */
  /* USER CODE BEGIN WHILE */
  while (1)
  {
    /* USER CODE END WHILE */

    /* USER CODE BEGIN 3 */
  }
  /* USER CODE END 3 */
}

/**
  * @brief System Clock Configuration
  * @retval None
  */
void SystemClock_Config(void)
{
  RCC_OscInitTypeDef RCC_OscInitStruct = {0};
  RCC_ClkInitTypeDef RCC_ClkInitStruct = {0};

  /** Supply configuration update enable
  */
  HAL_PWREx_ConfigSupply(PWR_LDO_SUPPLY);

  /** Configure the main internal regulator output voltage
  */
  __HAL_PWR_VOLTAGESCALING_CONFIG(PWR_REGULATOR_VOLTAGE_SCALE0);

  while(!__HAL_PWR_GET_FLAG(PWR_FLAG_VOSRDY)) {}

  /** Initializes the RCC Oscillators according to the specified parameters
  * in the RCC_OscInitTypeDef structure.
  */
  RCC_OscInitStruct.OscillatorType = RCC_OSCILLATORTYPE_HSI;
  RCC_OscInitStruct.HSIState = RCC_HSI_DIV1;
  RCC_OscInitStruct.HSICalibrationValue = RCC_HSICALIBRATION_DEFAULT;
  RCC_OscInitStruct.PLL.PLLState = RCC_PLL_ON;
  RCC_OscInitStruct.PLL.PLLSource = RCC_PLLSOURCE_HSI;
  RCC_OscInitStruct.PLL.PLLM = 4;
  RCC_OscInitStruct.PLL.PLLN = 60;
  RCC_OscInitStruct.PLL.PLLP = 2;
  RCC_OscInitStruct.PLL.PLLQ = 2;
  RCC_OscInitStruct.PLL.PLLR = 2;
  RCC_OscInitStruct.PLL.PLLRGE = RCC_PLL1VCIRANGE_3;
  RCC_OscInitStruct.PLL.PLLVCOSEL = RCC_PLL1VCOWIDE;
  RCC_OscInitStruct.PLL.PLLFRACN = 0;
  if (HAL_RCC_OscConfig(&RCC_OscInitStruct) != HAL_OK)
  {
    Error_Handler();
  }

  /** Initializes the CPU, AHB and APB buses clocks
  */
  RCC_ClkInitStruct.ClockType = RCC_CLOCKTYPE_HCLK|RCC_CLOCKTYPE_SYSCLK
                              |RCC_CLOCKTYPE_PCLK1|RCC_CLOCKTYPE_PCLK2
                              |RCC_CLOCKTYPE_D3PCLK1|RCC_CLOCKTYPE_D1PCLK1;
  RCC_ClkInitStruct.SYSCLKSource = RCC_SYSCLKSOURCE_PLLCLK;
  RCC_ClkInitStruct.SYSCLKDivider = RCC_SYSCLK_DIV1;
  RCC_ClkInitStruct.AHBCLKDivider = RCC_HCLK_DIV2;
  RCC_ClkInitStruct.APB3CLKDivider = RCC_APB3_DIV2;
  RCC_ClkInitStruct.APB1CLKDivider = RCC_APB1_DIV2;
  RCC_ClkInitStruct.APB2CLKDivider = RCC_APB2_DIV2;
  RCC_ClkInitStruct.APB4CLKDivider = RCC_APB4_DIV2;

  if (HAL_RCC_ClockConfig(&RCC_ClkInitStruct, FLASH_LATENCY_4) != HAL_OK)
  {
    Error_Handler();
  }
}

/* USER CODE BEGIN 4 */
/* Test if user code is programmed starting from address "APPLICATION_ADDRESS" */
uint8_t check_app(uint32_t app_addr)
{
  //info_printf("addr(0x%X): %#x\r\n", app_addr, ((*(volatile uint32_t *)app_addr) & 0x2FFE0000));
  return (((*(volatile uint32_t *)app_addr) & 0x2FFE0000) == 0x24000000);
}

typedef void (*pFunction)(void);
pFunction Jump_To_Application;
void Jump_to_APP(uint32_t app_addr)
{
  uint32_t JumpAddress;
  info_printf("jump to app: %#x\r\n", app_addr);
  __set_PRIMASK(1); // 关总中断  0打开总中断
  JumpAddress = *(volatile uint32_t *)(app_addr + 4);
  Jump_To_Application = (pFunction)JumpAddress;
  info_printf("jump %#x success \r\n", app_addr);
  __set_MSP(*(volatile uint32_t *)app_addr);            // 设置SP指针，复位指针
  // SCB->VTOR = FLASH_BASE | mem_cfg.current_app_address; // 设置向量表偏移寄存器
  //__set_CONTROL(0);                                     // 设置为特权级，使用MSP
  Jump_To_Application();                                // 开始跳转
}

void Jump_to_APP2()
{
  if (check_app(mem_cfg.current_app_address))
  {
    info_printf("CurrentAPP Check OK.\r\n");
    Jump_to_APP(mem_cfg.current_app_address);
  }
  else if (check_app(mem_cfg.app1_address))
  {
    info_printf("CurrentAPP Check Failed.\r\n");
    info_printf("APP1 Check OK.\r\n");
    mem_cfg.current_app_address = mem_cfg.app1_address;
    write_cfg();
    // read_cfg();
    Jump_to_APP(mem_cfg.app1_address);
  }
  else if (check_app(mem_cfg.app2_address))
  {
    info_printf("CurrentAPP Check Failed.\r\n");
    info_printf("APP1 Check Failed.\r\n");
    info_printf("APP2 Check OK.\r\n");
    mem_cfg.current_app_address = mem_cfg.app2_address;
    write_cfg();
    Jump_to_APP(mem_cfg.app2_address);
  }
  info_printf("CurrentAPP Check Failed.\r\n");
  info_printf("APP1 Check Failed.\r\n");
  info_printf("APP2 Check Failed.\r\n");
 
}

/*
 ******************************************************************************************************
 *    函 数 名: JumpToBootloader
 *    功能说明: 跳转到系统BootLoader
 *    形    参: 无
 *    返 回 值: 无
 ******************************************************************************************************
 */
static void JumpToBootloader(void)
{
  uint32_t i = 0;
  void (*SysMemBootJump)(void);        /* 声明一个函数指针 */
  __IO uint32_t BootAddr = 0x1FF09800; /* STM32H7的系统BootLoader地址 */

  /* 关闭全局中断 */
   __set_PRIMASK(1);

  /* 关闭滴答定时器，复位到默认值 */
  SysTick->CTRL = 0;
  SysTick->LOAD = 0;
  SysTick->VAL = 0;

  /* 设置所有时钟到默认状态，使用HSI时钟 */
  HAL_RCC_DeInit();

  /* 关闭所有中断，清除所有中断挂起标志 */
  for (i = 0; i < 8; i++)
  {
    NVIC->ICER[i] = 0xFFFFFFFF;
    NVIC->ICPR[i] = 0xFFFFFFFF;
  }

  /* 使能全局中断 */
  __set_PRIMASK(0);
  

  /* 跳转到系统BootLoader，首地址是MSP，地址+4是复位中断服务程序地址 */
  SysMemBootJump = (void (*)(void))(*((uint32_t *)(BootAddr + 4)));

  /* 设置主堆栈指针 */
  __set_MSP(*(uint32_t *)BootAddr);

  /* 在RTOS工程，这条语句很重要，设置为特权级模式，使用MSP指针 */
  __set_CONTROL(0);

  /* 跳转到系统BootLoader */
  SysMemBootJump();

  /* 跳转成功的话，不会执行到这里，用户可以在这里添加代码 */
  while (1)
  {
  }
}


/* USER CODE END 4 */

 /* MPU Configuration */

void MPU_Config(void)
{
  MPU_Region_InitTypeDef MPU_InitStruct = {0};

  /* Disables the MPU */
  HAL_MPU_Disable();

  /** Initializes and configures the Region and the memory to be protected
  */
  MPU_InitStruct.Enable = MPU_REGION_ENABLE;
  MPU_InitStruct.Number = MPU_REGION_NUMBER0;
  MPU_InitStruct.BaseAddress = 0x0;
  MPU_InitStruct.Size = MPU_REGION_SIZE_4GB;
  MPU_InitStruct.SubRegionDisable = 0x87;
  MPU_InitStruct.TypeExtField = MPU_TEX_LEVEL0;
  MPU_InitStruct.AccessPermission = MPU_REGION_NO_ACCESS;
  MPU_InitStruct.DisableExec = MPU_INSTRUCTION_ACCESS_DISABLE;
  MPU_InitStruct.IsShareable = MPU_ACCESS_SHAREABLE;
  MPU_InitStruct.IsCacheable = MPU_ACCESS_NOT_CACHEABLE;
  MPU_InitStruct.IsBufferable = MPU_ACCESS_NOT_BUFFERABLE;

  HAL_MPU_ConfigRegion(&MPU_InitStruct);
  /* Enables the MPU */
  HAL_MPU_Enable(MPU_PRIVILEGED_DEFAULT);

}

/**
  * @brief  This function is executed in case of error occurrence.
  * @retval None
  */
void Error_Handler(void)
{
  /* USER CODE BEGIN Error_Handler_Debug */
  /* User can add his own implementation to report the HAL error return state */
  __disable_irq();
  while (1)
  {
  }
  /* USER CODE END Error_Handler_Debug */
}

#ifdef  USE_FULL_ASSERT
/**
  * @brief  Reports the name of the source file and the source line number
  *         where the assert_param error has occurred.
  * @param  file: pointer to the source file name
  * @param  line: assert_param error line source number
  * @retval None
  */
void assert_failed(uint8_t *file, uint32_t line)
{
  /* USER CODE BEGIN 6 */
  /* User can add his own implementation to report the file name and line number,
     ex: printf("Wrong parameters value: file %s on line %d\r\n", file, line) */
  /* USER CODE END 6 */
}
#endif /* USE_FULL_ASSERT */
