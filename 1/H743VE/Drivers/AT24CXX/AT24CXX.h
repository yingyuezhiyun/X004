#ifndef AT24CXX_H__
#define AT24CXX_H__
/*****************************************
本驱动文件仅适配HAL库版本
******************************************/
#include "stm32h7xx_hal.h"	//链接HAL库



/* Exported constants (常量) --------------------------------------------------------*/


/* Exported macro (宏) --------------------------------------------------------------*/
#define READ_CMD                1
#define WRITE_CMD               0
#define USE_HARDWARE_IIC        1
#define x24C08//器件名称，x24C04、x24C08或x24C16
#define DEV_ADDR                0xA0                    //设备硬件地址

#ifdef x24C04
    #define PAGE_NUM            32                      //页数
    #define PAGE_SIZE           16                      //页面大小(字节)
    #define CAPACITY_SIZE       (PAGE_NUM * PAGE_SIZE)  //总容量(字节)
    #define ADDR_BYTE_NUM       1                       //地址字节个数
#endif
 
#ifdef x24C08
    #define PAGE_NUM            64                      //页数
    #define PAGE_SIZE           16                      //页面大小(字节)
    #define CAPACITY_SIZE       (PAGE_NUM * PAGE_SIZE)  //总容量(字节)
    #define ADDR_BYTE_NUM       1                       //地址字节个数
#endif
 
#ifdef x24C16
    #define PAGE_NUM            128                     //页数
    #define PAGE_SIZE           16                      //页面大小(字节)
    #define CAPACITY_SIZE       (PAGE_NUM * PAGE_SIZE)  //总容量(字节)
    #define ADDR_BYTE_NUM       1                       //地址字节个数
#endif

#define AT24C01     127
#define AT24C02     255
#define AT24C04     511
#define AT24C08     1023
#define AT24C16     2047
#define AT24C32     4095
#define AT24C64     8191
#define AT24C128    16383
#define AT24C256    32767  
//我使用的是AT24C04，容量4kbit（512byte）地址范围0~511

// /* I2C1 引脚 定义 */
// #define I2C1_SCL_GPIO_PORT              GPIOB
// #define I2C1_SCL_GPIO_PIN               GPIO_PIN_6
// #define I2C1_SCL_GPIO_CLK_ENABLE()      do{ __HAL_RCC_GPIOB_CLK_ENABLE(); }while(0)   /* PA口时钟使能 */

// #define I2C1_SDA_GPIO_PORT              GPIOB
// #define I2C1_SDA_GPIO_PIN               GPIO_PIN_7
// #define I2C1_SDA_GPIO_CLK_ENABLE()      do{ __HAL_RCC_GPIOB_CLK_ENABLE(); }while(0)   /* PA口时钟使能 */

// /* I2C1 相关定义 */
// #define I2C1_I2C                        I2C1
// #define I2C1_I2C_CLK_ENABLE()           do{ __HAL_RCC_I2C1_CLK_ENABLE(); }while(0)      /* I2C1时钟使能 */
// #define I2C1_I2C_CLK_DISABLE()          do{ __HAL_RCC_I2C1_CLK_DISABLE(); }while(0)     /* I2C1时钟失能 */

/* Exported variables (变量)---------------------------------------------------------*/

/* Exported functions (函数) ------------------------------------------------------- */

#define EE_TYPE AT24C08

#define IIC_CHECK_BYTE 0xA5


void AT24CXX_Init(void);

void AT24CXX_Write(uint16_t WriteAddr,uint8_t *pBuffer,uint16_t NumToWrite);

void AT24CXX_Read(uint16_t ReadAddr,uint8_t *pBuffer,uint16_t NumToRead);

uint8_t AT24CXX_Check(void);

void AT24CXX_PAGE_Check(void);

void AT24CXX_Write2(uint16_t WriteAddr, uint8_t *pBuffer, uint16_t NumToWrite);

enum
{
    IIC_Check_Fail,
    IIC_Check_OK,
    IIC_Check_OK_With_Empty,

};

#endif