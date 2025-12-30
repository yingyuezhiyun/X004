#include "AT24CXX.h"
#include "global_cfg.h"
#include "eeprom.h"
#include "usart.h"

uint8_t savePara()
{
    if (AT24CXX_Check() != IIC_Check_Fail)
    {
        AT24CXX_Write2(0, (uint8_t *)(&set_param), sizeof(set_param_t));
        //AT24CXX_Write2(sizeof(set_param_t), (uint8_t *)(Version2), 4);
        return 1;
    }
    return 0;
}

uint8_t saveBitInfo()
{
    if (AT24CXX_Check() != IIC_Check_Fail)
    {
        AT24CXX_Write2(sizeof(set_param_t), (uint8_t *)(Version2), 4);
        return 1;
    }
    return 0;
}

uint8_t saveWorkPara()
{
    if (AT24CXX_Check() != IIC_Check_Fail)
    {
        // AT24CXX_Write2(0, (uint8_t *)(&set_para), 25);
        return 1;
    }
    return 0;
}

uint8_t readPara()
{
    uint8_t check = AT24CXX_Check();
    if (check == IIC_Check_OK)
    {
        AT24CXX_Read(0, (uint8_t *)(&set_param), sizeof(set_param_t));
       // AT24CXX_Read(sizeof(set_param_t), (uint8_t *)(Version2), 4);
    }
    return check;
}

void iic_check()
{
    uint8_t data = 0xAA;
    uint8_t rd_data = 0;
    uint16_t addr = 0;

    uart_printf(&huart1, "addr");
    for (size_t i = 0; i < 16; i++)
    {
        uart_printf(&huart1, "\t%02x", i);
    }
    uart_printf(&huart1, "\r\n");
    for (size_t i = 0; i < EE_TYPE / 16; i++)
    {
        uart_printf(&huart1, "%03x:", i);
        for (size_t j = 0; j < 16; j++)
        {
            // AT24CXX_Write(i*16+j, &data, 1);
            AT24CXX_Write2(i * 16 + j, &data, 1);
            // Delay_ms(3);
            AT24CXX_Read(i * 16 + j, &rd_data, 1);
            uart_printf(&huart1, "\t%02x", rd_data);
        }
        uart_printf(&huart1, "\r\n");
    }
}
