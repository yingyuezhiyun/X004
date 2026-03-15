#include "bit_ini.h"
#include "main.h"
#include "eeprom.h"
#include "global_cfg.h"
#include "tim.h"
#include "LD_Ctrl.h"
#include "TEC_Ctrl.h"
#include "stdbool.h"
#include "AT24CXX.h"
#include "heat.h"
#include "max31865.h"
#include "heat.h"

// measure.c 中未在头文件公开的接口，硬件自检直接读取 ADC 值
extern float get_ld_curr(uint8_t ch);

extern void StartCtrlThread();

void hardwarecheck()
{
    // 简单自检流程：
    // 1) TEC：打开 TEC，设置小的升温输出电压，等待若干时间后读取温度是否上升
    // 2) LD：打开 LD，设置小电流（0.1A），读取实际电流是否有响应
    const float TEC_TEST_V = 2.0f;        // TEC 输出电压（V），正值期望使测温点升温
    const uint32_t TEC_TEST_MS = 3000;    // 等待时间 ms
    const float TEC_TEMP_THRESH = 0.3f;   // 温度上升阈值，单位 ℃

    const uint16_t LD_TEST_CUR_01A = 1;   // set_param.ld[].Cur 单位为 0.1A，1 => 0.1A
    const uint32_t LD_ON_WAIT_MS = 2500;  // 等待 LD 上电并切换完成（ms）
    const uint32_t LD_MEASURE_MS = 800;   // 测量窗口 （ms）
    const float LD_CUR_THRESH = 0.05f;    // 电流阈值 A

    // TEC 通道自检（4 路并行）
    {
        float t0[4];
        float t_now[4];
        uint8_t passed[4] = {0, 0, 0, 0};

        // 读取初始温度并打开所有 TEC
        for (uint8_t i = 0; i < 4; i++)
        {
            t0[i] = GET_TECx_Temp(i);
            t_now[i] = t0[i];

            // 打开 TEC 并设置输出电压（尝试使温度上升）
            set_param.tec[i].sw = WORK_ON;
            set_param.tec[i].OUT_V = TEC_TEST_V;
            SET_TEC_SW(i, WORK_ON);
            SET_TEC_OUT_Vol(i, TEC_TEST_V);
        }

        uint32_t start = GET_TickCount;
        // 在测试时间内轮询每路 TEC 的温度变化
        while ((GET_TickCount - start) < TEC_TEST_MS)
        {
            Delay_ms(200);
            for (uint8_t i = 0; i < 4; i++)
            {
                // 如果已经判定通过则跳过
                if (passed[i])
                    continue;

                t_now[i] = GET_TECx_Temp(i);

                // 温度读数不在合理范围，标记为 NTC 故障（热敏电阻异常）
                if ((t_now[i] < -80.0f) || (t_now[i] > 90.0f))
                {
                    // 通过 Work_Status 的 NTCx_ERR 位上报此故障
                    if (i == 0) Work_Status.NTC1_ERR = 1;
                    else if (i == 1) Work_Status.NTC2_ERR = 1;
                    else if (i == 2) Work_Status.NTC3_ERR = 1;
                    else if (i == 3) Work_Status.NTC4_ERR = 1;

                    // 标记为不通过（欠温位用于上报 TEC 故障）
                   set_param.tec[i].ErrStatus.content.UT = 1;
                   passed[i] = 1; // 跳过后续检测
                   continue;
                }

                // 检查温度是否上升达到阈值
                if ((t_now[i] - t0[i]) >= TEC_TEMP_THRESH)
                {
                    // 通过：清除欠温标志
                    set_param.tec[i].ErrStatus.content.UT = 0;
                    passed[i] = 1;
                }
            }
            // 若所有通道都已通过则可以提前退出
            if (passed[0] && passed[1] && passed[2] && passed[3])
                break;
        }

        // 对未通过的通道标记欠温
        for (uint8_t i = 0; i < 4; i++)
        {
            if (!passed[i])
            {
                set_param.tec[i].ErrStatus.content.UT = 1; // 欠温
            }
        }

        // 关闭所有 TEC，恢复 OUT_V
        for (uint8_t i = 0; i < 4; i++)
        {
            set_param.tec[i].OUT_V = 0;
            set_param.tec[i].sw = WORK_OFF;
            SET_TEC_OUT_Vol(i, 0);
            SET_TEC_SW(i, WORK_OFF);
        }
        Delay_ms(50);
    }

    // LD 通道自检（2 路并行）
    {
        const uint8_t BASE_SAMPLE_CNT = 5;
        float baseline[2] = {0, 0};
        float measured[2] = {0, 0};
        uint8_t is_on[2] = {0, 0};
        uint8_t passed[2] = {0, 0};
        const float LD_CUR_DELTA = 0.03f; // 期望电流增量阈值（A）

        // 先读取两路基线电流
        for (uint8_t b = 0; b < BASE_SAMPLE_CNT; b++)
        {
            baseline[0] += get_ld_curr(0);
            baseline[1] += get_ld_curr(1);
            Delay_ms(50);
        }
        baseline[0] /= BASE_SAMPLE_CNT;
        baseline[1] /= BASE_SAMPLE_CNT;

        // 同时设定期望电流（0.1A）并打开两路 LD
        for (uint8_t i = 0; i < 2; i++)
        {
            set_param.ld[i].Cur = LD_TEST_CUR_01A; // 单位 0.1A
            set_param.ld[i].sw = WORK_ON;
            SET_LD_SW(i, WORK_ON);
            SET_LD_Curr(i, (float)set_param.ld[i].Cur / 10.0f);
        }

        // 等待 LD 实际完成上电并切换 ON（LD 开关为 RESET 表示 ON），最长等待 LD_ON_WAIT_MS
        uint32_t on_start = GET_TickCount;
        while ((GET_TickCount - on_start) < LD_ON_WAIT_MS)
        {
            if (!is_on[0])
            {
                if (HAL_GPIO_ReadPin(LD_ON_OFF1_GPIO_Port, LD_ON_OFF1_Pin) == GPIO_PIN_RESET)
                {
                    is_on[0] = 1;
                }
            }
            if (!is_on[1])
            {
                if (HAL_GPIO_ReadPin(LD_ON_OFF2_GPIO_Port, LD_ON_OFF2_Pin) == GPIO_PIN_RESET)
                {
                    is_on[1] = 1;
                }
            }
            if (is_on[0] && is_on[1])
                break;
            Delay_ms(50);
        }

        // 对于未上电的通道直接标记欠流
        for (uint8_t i = 0; i < 2; i++)
        {
            if (!is_on[i])
            {
                set_param.ld[i].ErrStatus.content.UC = 1;
                passed[i] = 0;
            }
        }

        // 对已经上电的通道，在测量窗口内判断电流响应
        uint32_t start = GET_TickCount;
        while ((GET_TickCount - start) < LD_MEASURE_MS)
        {
            Delay_ms(100);
            for (uint8_t i = 0; i < 2; i++)
            {
                if (!is_on[i] || passed[i])
                    continue;
                measured[i] = get_ld_curr(i);
                if (((baseline[i] < LD_CUR_THRESH) && (measured[i] >= LD_CUR_THRESH)) || ((measured[i] - baseline[i]) >= LD_CUR_DELTA))
                {
                    set_param.ld[i].ErrStatus.content.UC = 0; // 通过
                    passed[i] = 1;
                }
            }
            if ((passed[0] || !is_on[0]) && (passed[1] || !is_on[1]))
                break;
        }

        // 未通过的通道标记欠流
        for (uint8_t i = 0; i < 2; i++)
        {
            if (!passed[i])
            {
                set_param.ld[i].ErrStatus.content.UC = 1;
            }
        }

        // 关闭 LD，恢复电流设定
        for (uint8_t i = 0; i < 2; i++)
        {
            set_param.ld[i].Cur = 0;
            set_param.ld[i].sw = WORK_OFF;
            SET_LD_Curr(i, 0);
            SET_LD_SW_OFF(i);
        }
        Delay_ms(50);
    }

}

void DefaultParams()
{
    for (size_t i = 0; i < 2; i++)
    {
        set_param.ld[i].sw = WORK_OFF;
        set_param.ld[i].Cur = 0;
        set_param.ld[i].calib_set.k = 1;
        set_param.ld[i].calib_set.b = 0;
        set_param.ld[i].calib_measure.k = 1;
        set_param.ld[i].calib_measure.b = 0;
        set_param.ld[i].HOC = 166;
        set_param.ld[i].Vol = 250;
    }
    for (size_t i = 0; i < 4; i++)
    {
        set_param.tec[i].sw = WORK_OFF;
        set_param.tec[i].MaxVol = 195;
        set_param.tec[i].Temp = 200;
        set_param.tec[i].PID.Kp = 38;
        set_param.tec[i].PID.Ki = 30;
        set_param.tec[i].PID.Kd = 20;
    }

    set_param.TRG_Type = TRG_INTER;
    set_param.Pulse_Type = PULSE_SPWM;
    set_param.T_Q.delay = 100;
    set_param.T_Q.sw = WORK_ON;
    set_param.Pulse_para.Num = 11;
    set_param.Pulse_para.Nor_Freq = 1000;
    set_param.Pulse_para.Width = 200;
    for (size_t i = 0; i < 11; i++)
    {
        set_param.Pulse_para.Interval[i] = 250;
    }
    set_param.LCM.Fan = WORK_OFF;
    set_param.LCM.MotorSpeed = 30000;
    set_param.LCM.PwrLimit = 150;
}

void RangeParams()
{
    for (size_t i = 0; i < 2; i++)
    {
        // if (set_param.ld[i].sw != WORK_ON)
        // {
        set_param.ld[i].sw = WORK_OFF;
        // }
    }
    for (size_t i = 0; i < 4; i++)
    {
        if (set_param.tec[i].sw != WORK_ON)
        {
            set_param.tec[i].sw = WORK_OFF;
        }
        if (set_param.tec[i].Temp < TEC_SET_MIN_TEMP)
        {
            set_param.tec[i].Temp = TEC_SET_MIN_TEMP;
        }
        else if (set_param.tec[i].Temp > TEC_SET_MAX_TEMP)
        {
            set_param.tec[i].Temp = TEC_SET_MAX_TEMP;
        }
        set_param.tec[i].PID.Resolution = 0.0004f;
    }
    if (set_param.Pulse_para.Num > 11)
    {
        set_param.Pulse_para.Num = 11;
    }
    set_param.Pulse_para.Nor_Freq = 1000;
    if(set_param.TRG_Type == TRG_OUT)
    {
        set_param.Pulse_Type = PULSE_SPWM;
    }


}

void DefaultStatus()
{
    memset(&Work_Status, 0, sizeof(Work_Status));
    for (size_t i = 0; i < 4; i++)
    {
        set_param.tec[i].ErrStatus.value = 0;
    }
    for (size_t i = 0; i < 2; i++)
    {
        set_param.ld[i].ErrStatus.value = 0;
    }
}

void DefaultRunningFlag()
{
    running_flag.ld_flags[0].value = 0;
    running_flag.ld_flags[1].value = 0;
    running_flag.pulse_flags.value = 0;
    running_flag.lcm_auto = 1;
}

uint8_t bit_init()
{

    DefaultParams();
    uint8_t result = 1;
    uint8_t read_result = readPara();
    Disable_LD1_EXIT_DET;
    Disable_LD2_EXIT_DET;
    Disable_TRG_IN_DET;
    STOP_FAN;
    MCU_PW_LOCK;
    DefaultStatus();
    SET_LD_SW_OFF(0);
    SET_LD_SW_OFF(1);
    for (size_t i = 0; i < 4; i++)
    {
        SET_TEC_SW(i, WORK_OFF);
    }
    if (read_result == IIC_Check_Fail)
    {
        Work_Status.EPPROM_ERR = 1;
        result = 0;
    }
    else if (read_result == IIC_Check_OK_With_Empty)
    {
        savePara();
    }
    RangeParams();

    // todo
    SET_LD_MAX_Curr(LD_CH_1, set_param.ld[0].HOC / 10.0);
    SET_LD_MAX_Curr(LD_CH_2, set_param.ld[1].HOC / 10.0);

    SET_LD_Vol(LD_CH_1, set_param.ld[0].Vol);
    SET_LD_Vol(LD_CH_2, set_param.ld[1].Vol);

    // pump_setting

    CalcPulse_SPWMParam();
    CalcPulse_NORParam();
    pulse_ini();
    SetInterTrgFreq();
    // SetPulseMode();

    set_param.HardWareTest = 1;   
    //hardwarecheck();
    set_param.HardWareTest = 0;
    // 自检完成后再启动控制线程
    StartCtrlThread();

    Enable_LD1_EXIT_DET;
    Enable_LD2_EXIT_DET;
    Enable_TRG_IN_DET;

    HAL_TIM_Base_Start(&htim3);
    Heat_Ini();

    // todo
    DefaultRunningFlag();

    return result;
}

void bit_check()
{
    Work_Status.TEC1_SW = set_param.tec[0].sw == WORK_ON;
    Work_Status.TEC2_SW = set_param.tec[1].sw == WORK_ON;
    Work_Status.TEC3_SW = set_param.tec[2].sw == WORK_ON;
    Work_Status.TEC4_SW = set_param.tec[3].sw == WORK_ON;
    Work_Status.LD1_SW = set_param.ld[0].sw == WORK_ON;
    Work_Status.LD2_SW = set_param.ld[1].sw == WORK_ON;
    Work_Status.Q_SW = set_param.T_Q.sw == WORK_ON;
    Work_Status.TRQ_Type = set_param.TRG_Type == TRG_OUT;
    Work_Status.Pulse_Type = set_param.Pulse_Type == PULSE_NOR;

    Work_Status.LD1_UC = set_param.ld[0].ErrStatus.content.UC;
    Work_Status.LD1_OC = set_param.ld[0].ErrStatus.content.OC;
    Work_Status.LD2_UC = set_param.ld[1].ErrStatus.content.UC;
    Work_Status.LD2_OC = set_param.ld[1].ErrStatus.content.OC;
    Work_Status.TEC1_UT = set_param.tec[0].ErrStatus.content.UT;
    Work_Status.TEC1_OT = set_param.tec[0].ErrStatus.content.OT;
    Work_Status.TEC2_UT = set_param.tec[1].ErrStatus.content.UT;
    Work_Status.TEC2_OT = set_param.tec[1].ErrStatus.content.OT;
    Work_Status.TEC3_UT = set_param.tec[2].ErrStatus.content.UT;
    Work_Status.TEC3_OT = set_param.tec[2].ErrStatus.content.OT;
    Work_Status.TEC4_UT = set_param.tec[3].ErrStatus.content.UT;
    Work_Status.TEC4_OT = set_param.tec[3].ErrStatus.content.OT;




    //todo 电源过热 电路故障 判断

}
