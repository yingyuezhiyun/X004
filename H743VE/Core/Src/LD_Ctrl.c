#include "main.h"
#include "stdint.h"
#include "string.h"
#include <stdlib.h>
#include "math.h"
#include "global_cfg.h"
#include "LD_Ctrl.h"
#include "DAC8568.h"
#include "tim.h"

#define OUT_TRG_CALC_CNT (3)

#define LD_START_DELAY_MS (2000)
#define LD_STOP_DELAY_MS (200)

#define LD_TRG_IN_MIN_FREQ (45)
#define LD_TRG_IN_MAX_FREQ (55)

#define UNDER_CURR_TIMEOUT (20 * 1000)
#define UNDER_CURR_THRESHOLD (0.95)

typedef struct
{
    uint8_t idx;
    float freq[OUT_TRG_CALC_CNT];
    struct
    {
        uint16_t tick;
        uint32_t time_ms;
    } now;
    struct
    {
        uint16_t tick;
        uint32_t time_ms;
    } last;
} LD_TRG_IN_param_t;

LD_TRG_IN_param_t LD_TRG_IN_param;

status_check_t UC_status[2]; // 欠流状态

// 延迟启动管理（非阻塞）: pending 表示已使能电源但尚未打开 ON_OFF
typedef struct
{
    uint8_t pending;
    uint32_t start_ms;
    uint8_t pending_off;
    uint32_t stop_ms;
} ld_start_delay_t;

static ld_start_delay_t ld_start_delay[2];

void SET_LD_SW(uint8_t ch, uint8_t sw)
{

    GPIO_TypeDef *GPIO_x;
    uint16_t GPIO_PIN_x;
    uint8_t state = (sw == WORK_ON ? 1 : 0);
    /* 延迟完成逻辑已合并到 WORK_ON 路径，故不再使用单独的 WORK_ON_COMPLETE 命令 */
    // 如果请求开机（WORK_ON），并且已经处于 pending 状态，则在此检查延迟是否到期以完成启动
    if (sw == WORK_ON)
    {
        if (ld_start_delay[ch].pending)
        {
            if ((GET_TickCount - ld_start_delay[ch].start_ms) >= LD_START_DELAY_MS)
            {
                // 延迟到期，完成启动（写入 RESET）
                LD_SW_ON(ch);
                ld_start_delay[ch].pending = 0;
                running_flag.ld_flags[ch].content.is_en = 1;
            }
            // 如果尚未到期，直接返回（保持 pending 状态），等待下一次调用检查
            return;
        }
    }
    else
    {
        if (ld_start_delay[ch].pending_off)
        {
            if ((GET_TickCount - ld_start_delay[ch].stop_ms) >= LD_STOP_DELAY_MS)
            {
                // 延迟到期，完成关闭
                LD_SW_OFF(ch);
                ld_start_delay[ch].pending_off = 0;
                running_flag.ld_flags[ch].content.is_en = 0;
            }
            // 如果尚未到期，直接返回（保持 pending_off 状态），等待下一次调用检查
            return;
        }
    }
    if (state)
    {
        // 立即打开电源使能
        LD_POWER_ON(ch);
        running_flag.ld_flags[ch].content.is_pwr_en = 1;
        // 如果已经处于 ON 状态，说明延迟已完成或设备已打开，清除 pending
        if (IS_LD_SW_ON(ch))
        {
            ld_start_delay[ch].pending = 0;
        }
        else
        {
            // 只有在尚未标记 pending 时才设置延迟起始时间，避免重复刷新 start_ms
            if (ld_start_delay[ch].pending == 0)
            {
                LD_SW_OFF(ch);
                running_flag.ld_flags[ch].content.is_en = 0;
                ld_start_delay[ch].pending = 1;
                ld_start_delay[ch].start_ms = GET_TickCount;
            }
        }
        ld_start_delay[ch].pending_off = 0;
    }
    else
    {

        // 立即关闭负载开关
        LD_POWER_OFF(ch);
        running_flag.ld_flags[ch].content.is_pwr_en = 0;
        // 如果已经处于 OFF 状态，说明延迟已完成或设备已关闭
        if (!IS_LD_SW_ON(ch))
        {
            ld_start_delay[ch].pending_off = 0;
            running_flag.ld_flags[ch].content.is_en = 0;
        }
        else
        {
            // 只有在尚未标记 pending_off 时才设置延迟起始时间，避免重复刷新 stop_ms
            if (ld_start_delay[ch].pending_off == 0)
            {
                LD_SW_ON(ch);
                running_flag.ld_flags[ch].content.is_en = 1;
                ld_start_delay[ch].pending_off = 1;
                ld_start_delay[ch].stop_ms = GET_TickCount;
            }
        }
        ld_start_delay[ch].pending = 0;
    }
}

void SET_LD_SW_OFF(uint8_t ch)
{
    LD_SW_OFF(ch);
    running_flag.ld_flags[ch].content.is_en = 0;
    LD_POWER_OFF(ch);
    running_flag.ld_flags[ch].content.is_pwr_en = 0;
}

void LD_OC_funexec(uint8_t ch)
{
    set_param.ld[ch].sw = WORK_OFF;
    set_param.ld[ch].ErrStatus.content.OC = 1;
    set_param.ld[ch].Cur_temp = 0;
    set_param.ld[ch].Cur = 0;
}

void Clear_LD_TRG_IN_Params()
{
    memset(&LD_TRG_IN_param, 0, sizeof(LD_TRG_IN_param));
}

/// @brief
/// @param GPIO_Pin
void HAL_GPIO_EXTI_Callback(uint16_t GPIO_Pin)
{
    if (GPIO_Pin == LD_OC_IN1_Pin)
    {
        SET_LD_SW_OFF(LD_CH_1);
        SET_LD_Curr(LD_CH_1, 0);
        LD_OC_funexec(LD_CH_1);
    }
    if (GPIO_Pin == LD_OC_IN2_Pin)
    {
        SET_LD_SW_OFF(LD_CH_2);
        SET_LD_Curr(LD_CH_2, 0);
        LD_OC_funexec(LD_CH_2);
    }

    if (GPIO_Pin == LD_TRG_IN_Pin && set_param.TRG_Type == TRG_OUT)
    {
        int dly = 1000;
        while (dly--)
            ;
        if (HAL_GPIO_ReadPin(LD_TRG_IN_GPIO_Port, LD_TRG_IN_Pin) == GPIO_PIN_RESET)
        {
            return;
        }
        LD_TRG_IN_param.last.tick = LD_TRG_IN_param.now.tick;
        LD_TRG_IN_param.last.time_ms = LD_TRG_IN_param.now.time_ms;
        LD_TRG_IN_param.now.time_ms = GET_TickCount;
        LD_TRG_IN_param.now.tick = __HAL_TIM_GetCounter(&htim3);
        float freq = 0;
        if (LD_TRG_IN_param.now.time_ms - LD_TRG_IN_param.last.time_ms > 65) // 大于65ms时 粗略估算
        {
            freq = 1000.0 / (LD_TRG_IN_param.now.time_ms - LD_TRG_IN_param.last.time_ms);
        }
        else
        {
            uint16_t tick = (LD_TRG_IN_param.now.tick - LD_TRG_IN_param.last.tick);
            freq = 1000000.0 / tick;
        }
        LD_TRG_IN_param.freq[LD_TRG_IN_param.idx++] = freq;
        LD_TRG_IN_param.idx %= OUT_TRG_CALC_CNT;
        uint8_t IS_In_range = 1;
        for (size_t i = 0; i < OUT_TRG_CALC_CNT; i++)
        {
            if (LD_TRG_IN_param.freq[i] < LD_TRG_IN_MIN_FREQ || LD_TRG_IN_param.freq[i] > LD_TRG_IN_MAX_FREQ)
            {
                IS_In_range = 0;
                break;
            }
        }
        if (IS_In_range && (set_param.ld[0].sw == WORK_ON || set_param.ld[1].sw == WORK_ON))
        {
            SetPulse_SPWMParam();
        }
        else
        {
            HAL_GPIO_WritePin(LD_TRG_OUT_GPIO_Port, LD_TRG_OUT_Pin, GPIO_PIN_RESET);
            HAL_GPIO_WritePin(Q_TRG_OUT_GPIO_Port, Q_TRG_OUT_Pin, GPIO_PIN_RESET);
        }
    }
}

void SET_LD_MAX_Curr(uint8_t ch, float curr)
{
    uint16_t dac = 0;

    if (curr < 0)
    {
        curr = 0;
    }
    // dac = curr * DAC_MAX_VAULE / 0.003 / 50 / DAC_REF_V;
    dac = curr * DAC_MAX_VAULE / DAC_REF_V * 0.003 * 50;
    switch (ch)
    {
    case LD_CH_1:
        SET_MAX_CUR1_DAC(dac);
        break;
    case LD_CH_2:
        SET_MAX_CUR2_DAC(dac);
        break;
    default:
        break;
    }
}

// 3mΩ*I*50=Vcurrset  Vcurrset最大为2.5V 最大可设置电流为16.67A
/// @brief
/// @param ch
/// @param curr
void SET_LD_Curr(uint8_t ch, float curr)
{
    uint16_t dac = 0;
    curr = curr * set_param.ld[ch].calib_set.k + set_param.ld[ch].calib_set.b;
    if (curr < 0)
    {
        curr = 0;
    }
    // dac = curr * DAC_MAX_VAULE / 0.003 / 50 / DAC_REF_V;
    dac = curr * DAC_MAX_VAULE / DAC_REF_V * 0.003 * 50;
    switch (ch)
    {
    case LD_CH_1:
        SET_CUR1_DAC(dac);
        break;
    case LD_CH_2:
        SET_CUR2_DAC(dac);
        break;
    default:
        break;
    }
}

// 设置范围 20.6V~33.8V
// Vout=((0.8-Vset)/6.8+0.8/1)*36+0.8
/// @brief
/// @param ch
/// @param Vol
void SET_LD_Vol(uint8_t ch, float Vol)
{
    uint16_t dac = 0;
    float Vset = 0.8 - ((Vol - 0.8) / 36.0 - 0.8) * 6.8;
    if (Vset < 0)
    {
        Vset = 0;
    }
    if (Vset > DAC_REF_V)
    {
        Vset = DAC_REF_V;
    }
    dac = Vset * DAC_MAX_VAULE / DAC_REF_V;
    switch (ch)
    {
    case LD_CH_1:
        // todo DAC标定系数
        // dac=
        SET_VOL1_DAC(dac);
        break;
    case LD_CH_2:
        SET_VOL2_DAC(dac);
        break;
    default:
        break;
    }
}

uint8_t status_deley_check(status_check_t *status_check, uint32_t time_out)
{
    uint8_t reslt = FLAG_RUNNING;

    switch (status_check->flag)
    {
    case FLAG_IDLE:
        status_check->flag = FLAG_RUNNING;
        status_check->startTickCount = GET_TickCount;
        break;
    case FLAG_RUNNING:
        if ((GET_TickCount - status_check->startTickCount) > time_out)
        {
            reslt = FLAG_OVERRANGE;
            status_check->flag = FLAG_OVERRANGE;
        }
        break;
    // case FLAG_DONE:
    default:
        reslt = FLAG_OVERRANGE;
        break;
    }

    return reslt;
}

void under_current_check(status_check_t *status_check, ld_setparam_t *sld, ld_measureparam_t *mld)
{

    if (mld->Cur < sld->Cur * UNDER_CURR_THRESHOLD)
    {
        if (status_deley_check(status_check, UNDER_CURR_TIMEOUT) == FLAG_OVERRANGE)
        {
            sld->ErrStatus.content.UC = 1;
        }
    }
    else
    {
        status_check->flag = FLAG_IDLE;
        sld->ErrStatus.content.UC = 0;
    }
}

void LD_Ctrl(uint8_t ld_ch, ld_setparam_t *sld, ld_measureparam_t *mld)
{

    uint8_t ld_sw = WORK_OFF;
    if (sld->sw == WORK_ON && sld->ErrStatus.value == 0 && sld->flag == FLAG_DONE && sld->Cur > 0)
    {
        if (sld->Cur_temp != sld->Cur)
        {
            sld->Cur_temp = sld->Cur;
            SET_LD_Curr(ld_ch, sld->Cur_temp / 10.0);
        }
        // under_current_check(&UC_status[ld_ch], sld, mld);
        ld_sw = WORK_ON;
    }
    else if (sld->Cur_temp != 0 && running_flag.ld_flags[ld_ch].content.is_en == 0) // 如果LD使能开关关闭，则将电流设置为0
    {
        sld->Cur_temp = 0;
        SET_LD_Curr(ld_ch, 0);
        UC_status[ld_ch].flag = FLAG_IDLE;
    }
    SET_LD_SW(ld_ch, ld_sw);
}

