#include "main.h"
#include "stdint.h"
#include "string.h"
#include <stdlib.h>
#include "math.h"
#include "global_cfg.h"
#include "TEC_Ctrl.h"
#include "DAC8568.h"
#include "LD_Ctrl.h"




#define TEC_OUT_MAX_DAC DAC_MAX_VAULE
// #define TEC_OUT_MIN_DAC (0.9 / DAC_REF_V * DAC_MAX_VAULE)
#define TEC_OUT_MIN_DAC (23592)

#define TEC_OUT_MAX_V (19.5)

#define TEC_TIMEOUT (1000 * 60)
#define TEC_INI_TIMEOUT (1000 * 60 * 5)

status_check_t OT_status[4];
status_check_t UT_status[4];
status_check_t TEC_status[4];

PID_Controller TEC_PID[4];
float TEC_OUT_V[4] = {0, 0, 0, 0};


// void pid_adjust(PID_Controller *pid)
// {
//     // 计算差值DIV
//     // float div = (set_para.power-measure_para.power)* set_para.cali_coef.power_curr.k + set_para.cali_coef.power_curr.b;
//     float div = (set_para.power - power_calculate_moving_average(&PowerCB)) * set_para.cali_coef.power_curr.k;
//     // float div = measure_para.power-set_para.power;
//     // 计算积分部分，累积误差
//     // float integral = pid->integral + div;
//     pid->integral += div;
//     // 计算微分部分，误差变化率
//     float derivative = div - pid->prev_error;
//     // PID输出
//     float output = pid->Kp * div + pid->Ki * pid->integral + pid->Kd * derivative;
//     // 电流值
//     // float current = set_para.current + output;
//     set_para.current += output;
//     if (set_para.current >= set_para.ld_max_curr)
//     {
//         set_para.current = set_para.ld_max_curr;
//     }
//     else if (set_para.current < 0)
//     {
//         set_para.current = 0;
//     }
//     // 是否可设最小分辨率
//     // float cur_dac_div = set_para.ld_type == LD_TYPE_1000mA ? 0.01907 : 0.004768;
//     // 1000mA => 19uA   250mA => 4.8uA
//     if (fabs(set_para.current - set_para.current_tmp) > set_para.curr_dac_div)
//     {
//         SET_LD_Curr_TIM(set_para.current);
//     }
//     else
//     {
//         set_para.current -= output;
//         pid->integral -= div;
//         return;
//     }
//     // 保存当前误差作为下次计算微分部分时的前一个误差
//     pid->prev_error = div;
//     // pid->integral = integral;
// }

float pid_adjust(PID_Controller *pid)
{
    // 计算差值DIV
    // float div = (set_para.power-measure_para.power)* set_para.cali_coef.power_curr.k + set_para.cali_coef.power_curr.b;
    // 计算积分部分，累积误差
    pid->integral += pid->div;

    // 计算微分部分，误差变化率
    float derivative = pid->div - pid->prev_error;

    // PID输出
    float output = pid->Kp * pid->div + pid->Ki * pid->integral / 100 + pid->Kd * derivative;

    if (fabs(output)< pid->Resolution) // 小于分辨率时，不累计
    {
        pid->integral -= pid->div;
        return 0;
    }
    if (pid->integral > 100)
    {
        pid->integral = 100;
    }
    else if (pid->integral < -100)
    {
        pid->integral = -100;
    }

    // 保存当前误差作为下次计算微分部分时的前一个误差
    pid->prev_error = pid->div;

    return output;
}

/*
    增量式 PID：返回输出增量 Δu（上层负责累加到实际控制量，例如 OUT_V）。
    公式（离散增量式近似）：
        Δu = Kp*(e[k]-e[k-1]) + Ki*e[k] + Kd*(e[k]-2*e[k-1]+e[k-2])
    说明：此处假定 Ki 已包含采样周期尺度；如需按采样周期缩放，请在调用前调整 Ki。
*/
float pid_adjust_incremental(PID_Controller *pid)
{
        float e = pid->div;
        float de = e - pid->prev_error;
        float dde = e - 2.0f * pid->prev_error + pid->prev_prev_error;

        float delta = pid->Kp * de + pid->Ki * e + pid->Kd * dde;

        if (fabs(delta) < pid->Resolution)
        {
                pid->prev_prev_error = pid->prev_error;
                pid->prev_error = e;
                return 0;
        }

        pid->prev_prev_error = pid->prev_error;
        pid->prev_error = e;

        return delta;
}

void RESET_PID(PID_Controller *pid)
{
    pid->div = 0;
    pid->prev_error = 0;
    pid->integral = 0;
}

// void TEC_PID_Init(uint8_t ch)
// {
//     switch (ch)
//     {
//     case TEC_CH_1:
//         TEC_PID[0].prev_error = 0;
//         TEC_PID[0].div = 0;
//         TEC_PID[0].integral = 0;
//         break;
//     case TEC_CH_2:
//         break;
//     case TEC_CH_3:
//         break;
//     case TEC_CH_4:
//         break;
//     default:
//         break;
//     }
// }



float get_TEC_MaxVol(ch)
{
    return set_param.tec[ch].MaxVol / 10.0;
}

// Vout=((0.8-Vset)/1.5+0.8/1)*14+0.8
// Vset = 0.8 - ((Vout - 0.8)/14 -0.8 )*1.5
// 范围0V~19.5V
/// @brief
/// @param ch
/// @param dac
void SET_TEC_OUT_Vol(uint8_t ch, float Vol)
{
    float Vol_tmp, Vset, maxVol;
    uint16_t dac_l, dac_r, dac_tmp;
    if (Vol > 0)
    {
        Vol_tmp = Vol;
    }
    else
    {
        Vol_tmp = -Vol;
    }
    // maxVol = fmin(TEC_OUT_MAX_V, get_TEC_MaxVol(ch));
    // if (Vol_tmp > maxVol)
    // {
    //     Vol_tmp = maxVol;
    // }

    Vset = 0.8 - ((Vol_tmp - 0.8) / 14.0 - 0.8) * 1.5;
    if (Vset < 0)
    {
        Vset = 0;
    }
    dac_tmp = Vset * DAC_MAX_VAULE / DAC_REF_V;

    if (Vol > 0)
    // if (Vol < 0)
    {
        dac_l = dac_tmp;
        dac_r = DAC_MAX_VAULE;
    }
    else
    {
        dac_l = DAC_MAX_VAULE;
        dac_r = dac_tmp;
    }

    switch (ch)
    {
    case TEC_CH_1:
        SET_U_L1_DAC(dac_l);
        SET_U_R1_DAC(dac_r);
        break;
    case TEC_CH_2:
        SET_U_L2_DAC(dac_l);
        SET_U_R2_DAC(dac_r);
        break;
    case TEC_CH_3:
        SET_U_L3_DAC(dac_l);
        SET_U_R3_DAC(dac_r);
        break;
    case TEC_CH_4:
        SET_U_L4_DAC(dac_l);
        SET_U_R4_DAC(dac_r);
        break;
    default:
        break;
    }
}

void SET_TEC_SW(uint8_t ch, uint8_t sw)
{

    GPIO_TypeDef *GPIO_x;
    uint16_t GPIO_PIN_x;
    uint8_t state = (sw == WORK_ON ? 1 : 0);
    switch (ch)
    {
    case TEC_CH_1:
        GPIO_x = TEC_ON_OFF1_GPIO_Port;
        GPIO_PIN_x = TEC_ON_OFF1_Pin;
        break;
    case TEC_CH_2:
        GPIO_x = TEC_ON_OFF2_GPIO_Port;
        GPIO_PIN_x = TEC_ON_OFF2_Pin;
        break;
    case TEC_CH_3:
        GPIO_x = TEC_ON_OFF3_GPIO_Port;
        GPIO_PIN_x = TEC_ON_OFF3_Pin;
        break;
    case TEC_CH_4:
        GPIO_x = TEC_ON_OFF4_GPIO_Port;
        GPIO_PIN_x = TEC_ON_OFF4_Pin;
        break;
    default:
        break;
    }
    HAL_GPIO_WritePin(GPIO_x, GPIO_PIN_x, state);
}

float get_tec_div(ch)
{
    return measure_param.tec[ch].Temp - set_param.tec[ch].Temp / 10.0;
}

uint8_t TEC_Check_UT(tec_setparam_t *stec, tec_measureparam_t *mtec)
{
    // return (stec->Temp / 10.0 > mtec->Temp + 3); // 欠温： 设置温度 > 测量温度+3
    return (mtec->Temp < stec->Temp / 10.0 - 3); // 欠温：测量温度 < 设置温度 -3
}

uint8_t TEC_Check_OT(tec_setparam_t *stec, tec_measureparam_t *mtec)
{
    // return (stec->Temp / 10.0 < mtec->Temp + 3); // 过温： 设置温度 < 测量温度+3
    return (mtec->Temp > stec->Temp / 10.0 + 3); // 过温： 测量温度 > 设置温度 +3
}

float tec_cur_limits[] = {14.0, 14.0, 7.0, 7.0};

uint8_t TEC_Cur_Limit(uint8_t tec_ch, tec_measureparam_t *mtec)
{
    return fabs(mtec->Cur) >= tec_cur_limits[tec_ch];
}

uint32_t tec_pid_ticket[4];
#define TEC_PID_PERIOD_MS (200)

void TEC_Work(uint8_t tec_ch, tec_setparam_t *stec, tec_measureparam_t *mtec)
{
    
   // SET_TEC_SW(tec_ch, stec->sw);
    if (stec->sw == WORK_ON && stec->ErrStatus.value == 0)
    {
        SET_TEC_SW(tec_ch, WORK_ON);
        // stec->PID.div = get_tec_div(tec_ch);
        if (GET_TickCount - tec_pid_ticket[tec_ch] > TEC_PID_PERIOD_MS)
        {
            tec_pid_ticket[tec_ch] = GET_TickCount;
           // stec->PID.div = mtec->Temp - stec->Temp / 10.0;
            stec->PID.div =  stec->Temp / 10.0 -mtec->Temp ;
            float OUT_V_inc = pid_adjust(&stec->PID) / 100;
            if (OUT_V_inc > 2)
            {
                OUT_V_inc = 2;
            }
            else if (OUT_V_inc<-2)
            {
                OUT_V_inc = -2;
            }
            

            if (TEC_Cur_Limit(tec_ch, mtec) && (fabs(stec->OUT_V + OUT_V_inc) > fabs(stec->OUT_V)))
            {
            }
            else
            {
                stec->OUT_V += OUT_V_inc;
                float tec_max_v = fmin(TEC_OUT_MAX_V, get_TEC_MaxVol(tec_ch));
                if (stec->OUT_V < -tec_max_v)
                {
                    stec->OUT_V = -tec_max_v;
                }
                else if (stec->OUT_V > tec_max_v)
                {
                    stec->OUT_V = tec_max_v;
                }
            }           
            SET_TEC_OUT_Vol(tec_ch, stec->OUT_V);
        }
    }
    else
    {
        SET_TEC_SW(tec_ch, WORK_OFF);
        stec->OUT_V = 0;
        SET_TEC_OUT_Vol(tec_ch, stec->OUT_V);
        RESET_PID(&stec->PID);
    }
}

void TEC_Ctrl(uint8_t tec_ch, tec_setparam_t *stec, tec_measureparam_t *mtec)
{

    TEC_Work(tec_ch, stec, mtec);
    if (stec->sw == WORK_OFF)
    {
        //SET_TEC_SW(tec_ch, WORK_OFF);
        //TEC_Work(tec_ch, stec, mtec);
        TEC_status[tec_ch].flag = FLAG_IDLE;
        OT_status[tec_ch].flag = FLAG_IDLE;
        UT_status[tec_ch].flag = FLAG_IDLE;
    }
    switch (TEC_status[tec_ch].flag)
    {
    case FLAG_IDLE:
        if (stec->sw == WORK_ON)
        {
            //SET_TEC_SW(tec_ch, WORK_ON);
            TEC_status[tec_ch].flag = FLAG_RUNNING;
            TEC_status[tec_ch].startTickCount = GET_TickCount;
        }
        break;
    case FLAG_RUNNING:
        if (TEC_Check_OT(stec, mtec) || TEC_Check_UT(stec, mtec))
        {
            if (status_deley_check(&TEC_status, TEC_INI_TIMEOUT) == FLAG_OVERRANGE)
            {
                // bit_info.err_info.tec_err = 1;
                if (TEC_Check_OT(stec, mtec))
                {
             
                    stec->ErrStatus.content.OT=1;
                }
                else
                {
                    stec->ErrStatus.content.UT=1;
                }
                TEC_status[tec_ch].flag = FLAG_OVERRANGE;
            }
        }
        else
        {
            TEC_status[tec_ch].flag = FLAG_DONE;
        }
        break;
    case FLAG_DONE:
        if (TEC_Check_OT(stec, mtec))
        {
            if (status_deley_check(&OT_status, TEC_TIMEOUT) == FLAG_OVERRANGE)
            {
                TEC_status[tec_ch].flag = FLAG_OVERRANGE;
                stec->ErrStatus.content.OT=1;
            }
            break;
        }
        else
        {
            OT_status[tec_ch].flag = FLAG_IDLE;
            stec->ErrStatus.content.OT=0;
        }
        if (TEC_Check_UT(stec, mtec))
        {
            if (status_deley_check(&UT_status, TEC_TIMEOUT) == FLAG_OVERRANGE)
            {
                TEC_status[tec_ch].flag = FLAG_OVERRANGE;
                stec->ErrStatus.content.UT=1;
            }
        }
        else
        {

            UT_status[tec_ch].flag = FLAG_IDLE;
            stec->ErrStatus.content.UT=0;
        }
        break;
    default:
        break;
    }
    stec->flag = UT_status[tec_ch].flag;
}






