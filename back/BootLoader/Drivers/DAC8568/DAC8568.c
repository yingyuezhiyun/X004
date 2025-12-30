#include "DAC8568.h"
#include "main.h"
#include "stm32h7xx.h"


#define DAC8568_TIMEOUT_MS 200

__weak void DAC8830_CS(uint8_t state)
{
    HAL_GPIO_WritePin(CS8830_GPIO_Port, CS8830_Pin, (GPIO_PinState)state);
}

void DAC8830_WRITE(uint16_t DAC_DATA)
{
		__HAL_SPI_DISABLE(&hspi1);
    uint32_t spi_reg_cr1 = (hspi1.Instance->CR1 & (~SPI_CR1_CPOL)) & (~SPI_CR1_CPHA);
    // uint32_t spi_reg_cr1_ = hspi1.Instance->CR1 & (~SPI_CR1_CPHA);
    //空闲时候 CLK为低，第一个边沿传输数据
    WRITE_REG(hspi1.Instance->CR1, spi_reg_cr1 | (SPI_POLARITY_LOW & SPI_CR1_CPOL)|(SPI_PHASE_1EDGE & SPI_CR1_CPHA));

    // HAL_SPI_MspDeInit(&hspi1);
    // MX_SPI1_Init2();
    //__set_PRIMASK(0);  /*  使能全局中断 */
    DAC8830_CS(1);
    DAC8830_CS(0);
    uint8_t data[2];

    data[0] = DAC_DATA >> 8;
    data[1] = DAC_DATA & 0xff;
    HAL_SPI_Transmit(&hspi1, data, 2, DAC8568_TIMEOUT_MS);
    DAC8830_CS(1);
    //空闲时候 CLK为高，第二个边沿传输数据
    WRITE_REG(hspi1.Instance->CR1, spi_reg_cr1 | (SPI_POLARITY_HIGH & SPI_CR1_CPOL)|(SPI_PHASE_2EDGE & SPI_CR1_CPHA));

    //  HAL_SPI_MspDeInit(&hspi1);
    //  MX_SPI1_Init();
}


