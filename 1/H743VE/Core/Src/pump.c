#include "main.h"
#include "stdint.h"
#include "string.h"
#include <stdlib.h>
#include "global_cfg.h"
#include "pump.h"
#include "fdcan.h"

pump_get_param_t pump_get_param;
pump_set_param_t pump_set_param;

void pump_parse(uint8_t *data, uint16_t len)
{
    if (len >= 8)
    {
        pump_get_param.MotorSpeed = (data[0] << 8) | data[1];
        pump_get_param.state = data[2];
        pump_get_param.DCVoltage = (data[3] << 8) | data[4];
        pump_get_param.DCCurrent = data[5];
        pump_get_param.Temp = data[6] /* / 2.0 */;
        pump_get_param.PwrLimit = data[7];

        measure_param.LCM.MotorEn = (pump_get_param.state == 1 ? WORK_ON : WORK_OFF);
        measure_param.LCM.MotorSpeed = pump_get_param.MotorSpeed;
        measure_param.LCM.DCCurrent = pump_get_param.DCCurrent;
        measure_param.LCM.DCVoltage = pump_get_param.DCVoltage;
        measure_param.LCM.Temp = pump_get_param.Temp * 5;
        measure_param.LCM.PwrLimit = pump_get_param.PwrLimit;
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