#include "DAC8568.h"
#include "main.h"
#include "stm32h7xx.h"
#include "spi.h"

#define DAC8568_TIMEOUT_MS 200

__weak void DAC8568_CS(uint8_t sel, uint8_t state)
{
    if (sel == DAC_SEL1)
    {
        HAL_GPIO_WritePin(DAC_CS1_GPIO_Port, DAC_CS1_Pin, (GPIO_PinState)state);
    }
    else
    {
        HAL_GPIO_WritePin(DAC_CS2_GPIO_Port, DAC_CS2_Pin, (GPIO_PinState)state);
    }
}

void DAC_SPI_WRITE(uint8_t sel, uint32_t data)
{
    DAC8568_CS(sel, 1);
    DAC8568_CS(sel, 0);
    uint8_t pData[4];
    pData[0] = (data >> 24) & 0XFF;
    pData[1] = (data >> 16) & 0XFF;
    pData[2] = (data >> 8) & 0XFF;
    pData[3] = (data >> 0) & 0XFF;

    HAL_SPI_Transmit(&hspi2, pData, 4, DAC8568_TIMEOUT_MS);
   // HAL_SPI_Transmit(&hspi2, &data, 4, DAC8568_TIMEOUT_MS);
    DAC8568_CS(sel, 1);
}

void DAC8568_pwr_down_all()
{
    uint32_t data = (1 << 26) | (1 << 8) | 0xf;
    DAC_SPI_WRITE(DAC_SEL1, data);
    DAC_SPI_WRITE(DAC_SEL2, data);
}

void DAC8568_pwr_up_all()
{
    uint32_t data = (1 << 26) | (1 << 8);
    DAC_SPI_WRITE(DAC_SEL1, data);
    DAC_SPI_WRITE(DAC_SEL2, data);
}

void DAC8568_Ini()
{
}

void DAC8558_use_inter_ref(uint8_t sel)
{
    uint32_t data = (1 << 27) | 1;
    DAC_SPI_WRITE(sel, data);
}

/// @brief 使用外部参考电压（上电默认）
/// @param sel
void DAC8558_use_out_ref(uint8_t sel)
{
    uint32_t data = (1 << 27);
    DAC_SPI_WRITE(sel, data);
}

void DAC8568_SET(uint8_t sel, uint8_t ch, uint16_t vaule)
{
    uint32_t data = (3 << 24) | (ch << 20) | (vaule << 4);
    DAC_SPI_WRITE(sel, data);
}