// #ifndef __GLOBAL_CFG_H__
// #define __GLOBAL_CFG_H__

#pragma once

#include "stdlib.h"
#include "stdint.h"
#include "usart.h"

// #define CFG_ADDR FLASH_BANK2_BASE
// #define BL_ADDR FLASH_BANK1_BASE
// #define APP1_ADDR (FLASH_BANK1_BASE + 0x20000)
// #define APP2_ADDR (FLASH_BANK2_BASE + 0x20000)

/*
BootLoader:	0x8000000		128k
APP1:		0x8020000		384k
CFG:		0x8080000		384k
APP2:		0x80E0000		128k
*/

#define BL_ADDR FLASH_BANK1_BASE
#define APP1_ADDR (FLASH_BANK1_BASE + 0x20000)
#define APP2_ADDR (FLASH_BANK1_BASE + 0x80000)
#define CFG_ADDR (FLASH_BANK1_BASE + 0xE0000)



#define Delay_ms(x) osDelay(x)
#define MutexLOCK osMutexAcquire(myMutex01Handle, portMAX_DELAY)
#define MutexUNLOCK osMutexRelease(myMutex01Handle)
#define DisableTASKS vTaskSuspendAll()

#define EnableTASKS xTaskResumeAll()

#define GET_TickCount xTaskGetTickCount()

#define TRG_INTER (0x55)
#define TRG_OUT (0xAA)

#define PULSE_SPWM (0x55)
#define PULSE_NOR (0xAA)

#define WORK_OFF (0x55)
#define WORK_ON (0xAA)

#define TEC_SET_MAX_TEMP (650)
#define TEC_SET_MIN_TEMP (150)

#define PUMP_SEND_ID (0x490)
#define PUMP_REV_ID (0x495)
#define FDCANTXRXLEN (8)

#define PULSE_MAX_SECTION (11)

enum CHECK_FLAGS
{
    FLAG_IDLE,
    FLAG_RUNNING,
    FLAG_DONE,
    FLAG_OVERRANGE,
    FLAG_CONFIG
};

typedef struct
{
    uint32_t current_app_address;
    uint32_t bootloader_address;
    uint32_t app1_address;
    uint32_t app2_address;
    uint32_t cfg_address;
} mem_cfg_t;
extern mem_cfg_t mem_cfg;
extern mem_cfg_t mem_cfg_mirror;

#pragma pack(1)

#define CLI_RX_BUFF 300
typedef struct
{

    volatile uint16_t usart_tail;
    volatile uint8_t usart_pktcplt; // 接收完整一包数据
    volatile uint8_t test;
    char rxbuf[CLI_RX_BUFF];

} cli_para_t;

enum
{
    FLAG_OTA_IDLE,
    FLAG_OTA_RUNNING,
    FLAG_OTA_LAST_DONE,
    FLAG_OTA_LAST_CHECK_ERR,
    FLAG_OTA_LAST_FAILED,
    FLAG_OTA_CUR_DONE,
    FLAG_OTA_CUR_CHECK_ERR,
    FLAG_OTA_CUR_FALIED
};

// PID控制器结构体
typedef struct
{
    float Kp;         // 比例增益
    float Ki;         // 积分增益
    float Kd;         // 微分增益
    float prev_error; // 上一次误差
    float integral;   // 积分累积
    float div;        // 偏差值
    float Resolution; // 分辨率
} PID_Controller;

typedef struct
{
    uint16_t Nor_Freq;     // 内部脉冲频率 单位Hz
    uint16_t Width;        // 脉冲宽度 单位us 范围 200-240
    uint16_t Num;          //
    uint16_t Interval[11]; // 单位 us 范围 240-660

} Pulse_param_t;

typedef struct
{

    uint8_t LD1_UC : 1;     // LD1驱动欠流
    uint8_t LD1_OC : 1;     // LD1驱动过流
    uint8_t LD2_UC : 1;     // LD2驱动欠流
    uint8_t LD2_OC : 1;     // LD2驱动过流
    uint8_t NTC1_ERR : 1;   // 热敏电阻1异常
    uint8_t NTC2_ERR : 1;   // 热敏电阻2异常
    uint8_t NTC3_ERR : 1;   // 热敏电阻3异常
    uint8_t NTC4_ERR : 1;   // 热敏电阻4异常
    uint8_t TEC1_UT : 1;    // TEC1欠温
    uint8_t TEC1_OT : 1;    // TEC1过温
    uint8_t TEC2_UT : 1;    // TEC2欠温
    uint8_t TEC2_OT : 1;    // TEC2过温
    uint8_t TEC3_UT : 1;    // TEC3欠温
    uint8_t TEC3_OT : 1;    // TEC3过温
    uint8_t TEC4_UT : 1;    // TEC4欠温
    uint8_t TEC4_OT : 1;    // TEC4过温
    uint8_t EPPROM_ERR : 1; // 参数存储错误
    uint8_t PWR_OT : 1;     // 电源过热
    uint8_t PWR_ERR : 1;    // 电路故障
    uint8_t TEC1_SW : 1;    // TEC1 开关 1：开启; 0：关闭
    uint8_t TEC2_SW : 1;    // TEC2 开关 1：开启; 0：关闭
    uint8_t TEC3_SW : 1;    // TEC3 开关 1：开启; 0：关闭
    uint8_t TEC4_SW : 1;    // TEC4 开关 1：开启; 0：关闭
    uint8_t Q_SW : 1;       // 调Q 开关 1：开启; 0：关闭
    uint8_t LD1_SW : 1;     // LD1 开关 1：开启; 0：关闭
    uint8_t LD2_SW : 1;     // LD2 开关 1：开启; 0：关闭
    uint8_t TRQ_Type : 1;   // 触发状态 1：外触发; 0：内触发
    uint8_t Pulse_Type : 1; // 脉冲类型 1：定频; 0：变频
    uint8_t remain : 4;     //
} Work_Status_t;

typedef union
{
    uint8_t value;
    struct
    {
        uint8_t ERR : 1; // 损坏
        uint8_t OT : 1;  // 过温
        uint8_t UT : 1;  // 欠温

    } content;
} tec_status_s;

typedef union
{
    uint8_t value;
    struct
    {
        uint8_t OC : 1; // 过流
        uint8_t UC : 1; // 欠流
        uint8_t OV : 1; // 过压
        uint8_t UV : 1; // 欠压
    } content;
} ld_status_s;

typedef struct
{
    uint8_t MotorEn;     // 电机开关
    uint16_t MotorSpeed; // 电机转速 1rpm
    uint16_t DCVoltage;  // 直流母线电压 0.1V
    uint8_t DCCurrent;   // 直流母线电流 0.1A
    int16_t Temp;        // 电机控制器温度 0.1℃
    uint8_t PwrLimit;    // 功率 1w
} M_LCM_t;

typedef struct
{
    float Cur; // 单位 A
    float Vol; // LD电压 V
} ld_measureparam_t;

typedef struct
{
    float Temp;  // 温度 0.1℃
    float Cur;   // 单位 0.1A
    float Power; // 输出功率 0.1W
} tec_measureparam_t;

typedef struct
{
    ld_measureparam_t ld[2];   // LD参数
    tec_measureparam_t tec[4]; // TEC参数
    struct
    {
        float PD;
        float Temp;
    } out;
    struct
    {
        float Cur; // 单位 A
        float Vol; // LD电压 V
        float Temp[4];
    } sys;
    float PWR_Temp;
    M_LCM_t LCM;
} measure_param_t;

typedef struct
{
    float k; // k
    float b; // b
} cali_coef_t;

typedef struct
{
    uint8_t sw;             // 开关
    uint8_t flag;           // 状态
    tec_status_s ErrStatus; // 错误状态
    int16_t Temp;           // 单位 0.1℃
    uint16_t MaxVol;        // 单位 0.1V
    cali_coef_t calib_temp; // 温度标定
    PID_Controller PID;     // PID
    float OUT_V;            // 输出控制量
} tec_setparam_t;

typedef struct
{
    uint8_t sw;                // 开关
    uint8_t flag;              // 状态
    ld_status_s ErrStatus;     // 错误状态
    uint16_t Cur;              // 单位 0.1A
    uint16_t Cur_temp;         // 电流设定缓冲（真实设定值） 单位 0.1A
    uint16_t Vol;              // 单位 0.1V
    uint16_t HOC;              // 硬件过流 单位 0.1A
    cali_coef_t calib_set;     // 设置电流
    cali_coef_t calib_measure; // 测试电流
} ld_setparam_t;

typedef struct
{
    uint8_t sw;     // 开关
    uint16_t delay; // 调Q延时 单位us
} T_Q_param_t;

typedef struct
{
    uint8_t PwrEn;       // 电泵供电开关
    uint8_t MotorEn;     // 电机开关
    uint16_t MotorSpeed; // 转速 1rpm 0 ～30000
    uint8_t Fan;         // 风扇开关
    uint8_t PwrLimit;    // 功率限制 W 0~250
} S_LCM_t;

typedef struct
{
    ld_setparam_t ld[2];      // LD参数
    tec_setparam_t tec[4];    // TEC参数
    T_Q_param_t T_Q;          // 调Q参数
    uint8_t TRG_Type;         // 触发类型
    uint8_t Pulse_Type;       // 脉冲类型
    Pulse_param_t Pulse_para; // 脉冲参数
    cali_coef_t PD_calib;     //
    S_LCM_t LCM;              // 液冷模块控制
    uint8_t HardWareTest;     // 硬件测试模式   
} set_param_t;

typedef struct
{
    union
    {
        struct
        {
            uint8_t inter_spwm : 2;
            uint8_t inter_nor : 2;
            uint8_t out_spwm : 2;
            uint8_t others : 2;
        } content;
        uint8_t value;
    } pulse_flags;
    union
    {
        struct
        {
            uint8_t is_pwr_en : 1;
            uint8_t is_en : 1;
        } content;
        uint8_t value;
    } ld_flags[2];
} running_flag_t;

#pragma unpack()
extern cli_para_t cli_para;
extern set_param_t set_param;
extern measure_param_t measure_param;
extern Work_Status_t Work_Status;
extern running_flag_t running_flag;
extern char Version2[];

// #define BEBUG_UART

// #endif