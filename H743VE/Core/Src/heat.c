#include "main.h"
#include "stdint.h"
#include "string.h"
#include <stdlib.h>
#include "math.h"
#include "global_cfg.h"
#include "tim.h"

extern float pid_adjust(PID_Controller *pid);
extern float pid_adjust_incremental(PID_Controller *pid);
extern void RESET_PID(PID_Controller *pid);

#define SET_HEAT_PWM_DutyCycle(x) Set_PWM_DutyCycle(&htim1, TIM_CHANNEL_1, x)

void Heat_Ini()
{
    __HAL_TIM_SET_AUTORELOAD(&htim1, 1000);
    //__HAL_TIM_SET_COMPARE(&htim1, TIM_CHANNEL_1, 0);
    SET_HEAT_PWM_DutyCycle(0);
    //HAL_TIM_Base_Start(&htim1);
    HAL_TIM_PWM_Start(&htim1, TIM_CHANNEL_1);
}

#define HEAT_TMEP_THR (-30)

PID_Controller Heat_PID = {
    .Kp = 5,
    .Ki = 500,
    .Kd = 20,

};

static uint32_t heat_pid_ticket = 0;
static float PWM_DutyCycle;
#define HEAT_PID_PERIOD_MS (1000)

void Heat_Ctrl()
{

    if (measure_param.PWR_Temp < HEAT_TMEP_THR)
    {
        if (GET_TickCount - heat_pid_ticket > HEAT_PID_PERIOD_MS)
        {
            heat_pid_ticket = GET_TickCount;
            Heat_PID.div = HEAT_TMEP_THR - measure_param.PWR_Temp;
            float inv = pid_adjust_incremental(&Heat_PID);
            if (inv > 10)
            {
                inv = 10;
            }
            else if (inv < -10)
            {
                inv = -10;
            }
            PWM_DutyCycle += inv;
            if (PWM_DutyCycle > 100)
            {
                PWM_DutyCycle = 100;
            }
            SET_HEAT_PWM_DutyCycle(PWM_DutyCycle);
        }
        // TODO
    }
    else
    {
        SET_HEAT_PWM_DutyCycle(0);
        RESET_PID(&Heat_PID);
        PWM_DutyCycle = 0;
    }
}