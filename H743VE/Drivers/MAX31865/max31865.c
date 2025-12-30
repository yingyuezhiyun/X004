
#include "max31865.h"
#include "main.h"
#include "spi.h"
#include "math.h"
enum
{

    MAX31865_SEL1,
    MAX31865_SEL2,
    MAX31865_SEL3,
    MAX31865_SEL4,
};

__weak void MAX31865_CS(uint8_t sel, uint8_t state)
{
    switch (sel)
    {
    case MAX31865_SEL1:
        HAL_GPIO_WritePin(Temp_CS1_GPIO_Port, Temp_CS1_Pin, (GPIO_PinState)state);
        break;
    case MAX31865_SEL2:
        HAL_GPIO_WritePin(Temp_CS2_GPIO_Port, Temp_CS2_Pin, (GPIO_PinState)state);
        break;
    case MAX31865_SEL3:
        HAL_GPIO_WritePin(Temp_CS3_GPIO_Port, Temp_CS3_Pin, (GPIO_PinState)state);
        break;
    case MAX31865_SEL4:
        HAL_GPIO_WritePin(Temp_CS4_GPIO_Port, Temp_CS4_Pin, (GPIO_PinState)state);
        break;
    default:
        break;
    }
}

#define MAX31865_TIMEOUT_MS 200

/**
 * @brief       读取MAX31865寄存器
 * @param       reg：寄存器
 * @retval      data：读取到的寄存器的值
 */
uint8_t MAX31865_ReadByte(uint8_t sel, uint8_t reg)
{
    MAX31865_CS(sel, 1);
    MAX31865_CS(sel, 0);
    uint8_t pTxData[2] = {reg & 0x7F};
    uint8_t pRxData[2];
    HAL_SPI_TransmitReceive(&hspi4, pTxData, pRxData, 2, MAX31865_TIMEOUT_MS);
    MAX31865_CS(sel, 1);
    return pRxData[1];
}

uint16_t MAX31865_ReadUint16(uint8_t sel, uint8_t reg)
{
    MAX31865_CS(sel, 1);
    MAX31865_CS(sel, 0);
    uint8_t pTxData[3] = {reg & 0x7F};
    uint8_t pRxData[3];
    HAL_SPI_TransmitReceive(&hspi4, pTxData, pRxData, 2, MAX31865_TIMEOUT_MS);
    MAX31865_CS(sel, 1);
    return (pRxData[1] << 8) | pRxData[2];
}

/**
 * @brief       写MAX31865寄存器
 * @param       reg：寄存器
 * @param       data：写寄存器的值
 * @retval      无
 */
void MAX31865_WriteByte(uint8_t sel, uint8_t reg, uint8_t data)
{
    MAX31865_CS(sel, 1);
    MAX31865_CS(sel, 0);
    uint8_t pData[2];
    pData[0] = reg | 0x80;
    pData[1] = reg | data;
    HAL_SPI_Transmit(&hspi4, pData, 2, MAX31865_TIMEOUT_MS);
    MAX31865_CS(sel, 1);
}
float PT_ref[4]={3900,3900,3900,3900};
float calib_temp(uint16_t data,uint8_t sel)
{
    float Rt;
    float Rt0 = 1000; // PT1000
    float Z1, Z2, Z3, Z4, temp;
    float a = 3.9083e-3;
    float b = -5.775e-7;
    float rpoly;
    Rt = (float)data / 32768.0f * PT_ref[sel]; /* 计算RTD的电阻值：Rrtd = （ADC Code * Rref）/（2^15）*/

    // Rt = (float)data / 32768.0f * PT1000_Resistance_Ref; /* 计算RTD的电阻值：Rrtd = （ADC Code * Rref）/（2^15）*/

    // printf("Rt=0x%.1f\r\n",Rt);

    /*************根据数据手册编写**********/

#if 0
    Z1 = -a;
    Z2 = a * a - 4 * b;
    Z3 = 4 * b / Rt0;
    Z4 = 2 * b;

    temp = Z2 + Z3 * Rt;
    temp = (sqrt(temp) + Z1) / Z4;

    if (temp >= 0)
        return temp;

    rpoly = Rt;
    temp = -242.02;
    temp += 2.2228 * rpoly;
    rpoly *= Rt; // square
    temp += 2.5859e-3 * rpoly;
    rpoly *= Rt; // ^3
    temp -= 4.8260e-6 * rpoly;
    rpoly *= Rt; // ^4
    temp -= 2.8183e-8 * rpoly;
    rpoly *= Rt; // ^5
    temp += 1.5243e-10 * rpoly;
#else
    temp = (Rt - 1000.0f) / 3.85055f;
#endif
    return temp;
}

float Max31865_Read_Temperature(uint8_t sel)
{
    uint16_t Max31865_Adc_Value;
    float Temperature;
    //Max31865_Adc_Value = MAX31865_ReadUint16(sel, MAX31865_RTD_MSB_REG) >> 1; // 获取15位有效数据
    Max31865_Adc_Value = (MAX31865_ReadByte(sel,MAX31865_RTD_MSB_REG)<<7)|((MAX31865_ReadByte(sel,MAX31865_RTD_LSB_REG))>>1);
    Temperature = calib_temp(Max31865_Adc_Value,sel);
    return Temperature;
}

/// @brief 设置配置寄存器
/// @param data MAX31865_Init_2_4_Line or MAX31865_Init_3_Line
void MAX31865_Set_Configuration(uint8_t data)
{
    uint8_t Cfg_data;
    for (size_t i = 0; i < 4; i++)
    {
        MAX31865_WriteByte(i, MAX31865_CONFIG_REG, data);     /* 设置配置寄存器 */
        Cfg_data = MAX31865_ReadByte(i, MAX31865_CONFIG_REG); /* 读取配置寄存器 */
        // printf("MAX31865_CONFIG_REG: %x\r\n", Cfg_data);
    }
}

/// @brief 清空故障状态寄存器
/// @param
void MAX31865_Clear_Fault_Status(void)
{
    uint8_t data;
    uint8_t config_data;

    for (size_t i = 0; i < 4; i++)
    {
        data = MAX31865_ReadByte(i, MAX31865_FAULT_STATUS_REG); /* 读取故障状态寄存器 */
        // printf("MAX31865_FAULT_STATUS_REG: %x\r\n", data);
        config_data = MAX31865_ReadByte(i, MAX31865_CONFIG_REG);        /* 读取配置寄存器 */
        MAX31865_WriteByte(i, MAX31865_CONFIG_REG, config_data | 0x02); /* 清除故障状态位 */
        data = MAX31865_ReadByte(i, MAX31865_FAULT_STATUS_REG);         /* 读取故障状态寄存器 */
                                                                        // printf("MAX31865_FAULT_STATUS_REG: %x\r\n", data);
    }
}
