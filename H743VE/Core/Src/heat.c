#include "main.h"
#include "stdint.h"
#include "string.h"
#include <stdlib.h>
#include "math.h"
#include "global_cfg.h"
#include "tim.h"

extern float pid_adjust(PID_Controller *pid);
extern void RESET_PID(PID_Controller *pid);

#define SET_HEAT_PWM_DutyCycle(x) Set_PWM_DutyCycle(&htim1, TIM_CHANNEL_1, x)

void Heat_Ini()
{
    __HAL_TIM_SET_AUTORELOAD(&htim1, 1000);
    //__HAL_TIM_SET_COMPARE(&htim1, TIM_CHANNEL_1, 0);
    SET_HEAT_PWM_DutyCycle(0);
    HAL_TIM_Base_Start(&htim1);
}

#define HEAT_TMEP_THR (-30)

PID_Controller Heat_PID = {
    .Kp = 0.5,
    .Ki = 0.01,
    .Kd = 0,

};

void Heat_Ctrl()
{
    static float PWM_DutyCycle;

    if (measure_param.PWR_Temp < HEAT_TMEP_THR)
    {
        // TODO
        Heat_PID.div = HEAT_TMEP_THR - measure_param.PWR_Temp;
        PWM_DutyCycle += pid_adjust(&Heat_PID) / 100;
        if (PWM_DutyCycle > 100)
        {
            PWM_DutyCycle = 100;
        }
        SET_HEAT_PWM_DutyCycle(PWM_DutyCycle);
    }
    else
    {
        SET_HEAT_PWM_DutyCycle(0);
        RESET_PID(&Heat_PID);
        PWM_DutyCycle = 0;
    }
}