#include "main.h"
#include "stdint.h"
#include "string.h"
#include <stdlib.h>
#include "global_cfg.h"
#include "pump.h"
#include "fdcan.h"

pump_get_param_t pump_get_param;
pump_set_param_t pump_set_param;

void pump_parse(uint32_t id, uint8_t *data, uint16_t len)
{
    if (id == PUMP_SEND_ID && len >= 8)
    {
        pump_get_param.MotorSpeed = (data[0] << 8) | data[1];
        pump_get_param.state = data[2];
        pump_get_param.DCVoltage = (data[3] << 8) | data[4];
        pump_get_param.DCCurrent = data[5];
        // pump_get_param.Temp = data[6] /* / 2.0 */;
        pump_get_param.PwrLimit = data[7];

        measure_param.LCM.MotorEn = (pump_get_param.state & 0x01) == 1 ? WORK_ON : WORK_OFF;
        measure_param.LCM.MotorSpeed = pump_get_param.MotorSpeed;
        measure_param.LCM.DCCurrent = pump_get_param.DCCurrent;
        measure_param.LCM.DCVoltage = pump_get_param.DCVoltage;
        // measure_param.LCM.Temp = pump_get_param.Temp * 5;
        measure_param.LCM.PwrLimit = pump_get_param.PwrLimit;
        measure_param.LCM.state = pump_get_param.state;
    }
    else if (id == PUMP_SEND_ID_EX && len >= 8)
    {
        // 处理扩展ID的数据
        if (data[7] == 0x1)
        {
            //data[0]; 供液压力
            // pump_get_param.Temp = data[1]; // 供液温度
            pump_get_param.Temp = data[2];//环境温度
            //data[3];水泵状态
            //data[4];目标转速
            //data[5];当前运行时间 小时数高8位
            //data[6];当前运行时间 小时数低8位
            //data[7];帧标志

            measure_param.LCM.Temp = pump_get_param.Temp;
        }
    }
}

void pump_setting(pump_set_param_t *p)
{
    // PumpTxFrame.Data[0]=

    memset(PumpTxFrame.Data, 0, sizeof(PumpTxFrame.Data));
    PumpTxFrame.Data[0] = p->MotorSpeed >> 8;
    PumpTxFrame.Data[1] = p->MotorSpeed & 0xff;
    PumpTxFrame.Data[2] = p->state;
    PumpTxFrame.Data[7] = p->PwrLimit;

    fdcan_send(&PumpTxFrame);
}

#define PUMP_CTRL_INTERVAL_MS (5000)
static uint32_t pump_ctrl_ticket = 0;

void pump_ctrl()
{
    if (running_flag.lcm_auto)
    {
        if (GET_TickCount - pump_ctrl_ticket > PUMP_CTRL_INTERVAL_MS)
        {
            pump_ctrl_ticket = GET_TickCount;
            S_LCM_t *lcm = &set_param.LCM;
            // 用电路板温度判定，就用小于5度关闭水泵和风扇，大于5度打开水泵和风扇
            if (measure_param.PWR_Temp >= 5)
            {
                lcm->Fan = WORK_ON;
                lcm->PwrEn = WORK_ON;
                pump_set_param.MotorSpeed = set_param.LCM.MotorSpeed;
                pump_set_param.state = 1;
                START_FAN;
                HAL_GPIO_WritePin(LCVG_ONOFF_GPIO_Port, LCVG_ONOFF_Pin, 1);
            }
            else
            {

                lcm->PwrEn = WORK_OFF;
                lcm->Fan = WORK_OFF;
                pump_set_param.MotorSpeed = 0;
                pump_set_param.state = 0;
                STOP_FAN;
                HAL_GPIO_WritePin(LCVG_ONOFF_GPIO_Port, LCVG_ONOFF_Pin, 0);
            }
            pump_set_param.PwrLimit = 250;
            lcm->PwrLimit = pump_set_param.PwrLimit;
            lcm->MotorEn = pump_set_param.state == 1 ? WORK_ON : WORK_OFF;
            lcm->MotorSpeed = pump_set_param.MotorSpeed;
            pump_setting(&pump_set_param);
        }
    }
}
