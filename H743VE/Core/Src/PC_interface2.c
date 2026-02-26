#include "main.h"
#include "global_cfg.h"
#include "usart.h"
#include "string.h"
#include "LD_Ctrl.h"
#include "TEC_Ctrl.h"
#include "pump.h"
#include "tim.h"
#include "PC_interface.h"
#include "cmsis_os2.h"
#include "upgrade.h"

#define MAX_SIZE (256)
#define CMD_HEAD (0x545A)
#define CMD_TAIL (0xFE5A)
#define PC_ID (0x14)
#define MCU_ID (0x1c)

// char Version[] = "X004_001";
// uint64_t BuildTime = 202509112205;

static char Version2[] = {0, 0, 0, 3};//V0.0.0.2

typedef struct
{
    uint16_t Temp; // 设置温度 单位 0.1℃
    uint16_t V;    // 最高电压 单位 0.1V
    uint16_t Mode; // 模式
} tec_packet_t;

typedef struct
{
    int16_t k; // 单位 0.01
    int16_t b; // 单位 0.01
} kb_packet_t;

typedef struct
{
    uint16_t p; // 单位 0.01
    uint16_t i; // 单位 0.01
    uint16_t d; // 单位 0.01
} pid_packet_t;

typedef struct
{
    uint16_t Num;
    uint16_t Interval[11]; // 单位 us 范围 240-660
} Pulse_param_packet_t;

enum
{
    P_S_LD1_S_Cur = 0x30,   /* 设置电流通道1 10～150代表1.0A～15.0A */
    P_S_PULSE_WIDTH = 0x33, /* 设置脉宽 200us～240us */
    P_S_Q_DELAY = 0x34,     /* 设置调Ｑ延时 1us～300us */
    P_S_TRG_TYPE = 0x35,    /* 设置触发模式 0x55内触发；0xAA外触发 */
    P_S_LD1_SW = 0x36,      /* 设置LD电流开关通道1 0x55:关LD电流；0xAA：开LD电流。上电默认关 */
    P_S_Q_SW = 0x37,        /* 设置Q脉冲开关 0x55:关Ｑ脉冲；0xAA：开Ｑ脉冲。上电默认开 */
    P_S_TEC1_SW = 0x38,     /* 设置TEC1开关 0x55：关TEC1；0xAA：开TEC1。上电默认开 */
    P_S_TEC2_SW = 0x39,     /* 设置TEC2开关 0x55：关TEC2；0xAA：开TEC2。上电默认开 */
    P_S_TEC1_PARA = 0x3A,   /* 设置第1路TEC：工作温度、限压、模式 设置TEC1参数，共2个参数，每个参数2个字节：1、工作温度（150～650，代表15.0℃～65.0℃）2、限压 */
    P_S_TEC2_PARA = 0x3B,   /* 设置第2路TEC 工作温度、限压、模式 设置TEC1参数，共2个参数，每个参数2个字节：1、工作温度（150～650，代表15.0℃～65.0℃）2、限压 */
    P_S_PULSE_PARA = 0x3C,  /* 设置子脉冲参数，共12个参数，每个参数2个字节：1、子脉冲个数（取值范围1~12）2~11、脉冲间隔1（取值范围240~660）*/

    P_G_PULSE_WIDTH = 0x50, /* 查询脉宽设定值 */
    P_G_Q_DELAY = 0x51,     /* 查询调Ｑ延时设定值 */
    P_G_TRG_TYPE = 0x52,    /* 查询触发模式 */
    P_G_LD1_S_Cur = 0x53,   /* 查询LD电流设定值通道1 */
    P_G_TEC1_SW = 0x55,     /* 查询TEC1开关 */
    P_G_TEC2_SW = 0x56,     /* 查询TEC2开关 */

    P_G_TEC1_PARA = 0x57,  /* 查询第1路TEC：温度、限压设定值 */
    P_G_TEC2_PARA = 0x58,  /* 查询第2路TEC：温度、限压设定值 */
    P_G_PULSE_PARA = 0x59, /* 查询子脉冲个数及脉冲间隔设定值 */

    P_G_LD1_M_Cur = 0x5A,   /* 查询LD电流检测值通道1 */
    P_G_LD1_M_V = 0x5B,     /* 查询负载电压检测值通道1 */
    P_G_PWR_TEMP = 0x5C,    /* 查询电源温度检测值 */
    P_G_OUT_PD = 0x5D,      /* 查询外部输入电平1（PD）检测值 */
    P_G_OUT_TEMP = 0x5E,    /* 查询外部输入电平2（温度）检测值 */
    P_G_TEC1_M_TEMP = 0x5F, /* 查询第1路检测温度检测值 */
    P_G_TEC1_M_PW = 0x70,   /* 查询第1路TEC输出功率检测值 */
    P_G_TEC2_M_TEMP = 0x71, /* 查询第2路检测温度检测值 */
    P_G_TEC2_M_PW = 0x72,   /* 查询第2路TEC输出功率检测值 */

    P_G_WRK_STA = 0x73, /* 查询工作状态 */
    P_G_ALL_SET = 0x74, /* 查询所有设定参数 */
    P_G_ALL_M = 0x75,   /* 查询所有检测参数 */
    P_S_UPGRADE = 0x76, /* 在线程序升级 */

    P_S_LD2_S_Cur = 0x90, /* 设置电流通道2 10～150代表1.0A～15.0A */

    P_S_LD2_SW = 0x96, /* 设置LD电流开关通道2 0x55:关LD电流；0xAA：开LD电流。上电默认关 */

    P_S_TEC3_SW = 0x98, /* 设置TEC3开关 0x55：关TEC3；0xAA：开TEC3。上电默认开 */
    P_S_TEC4_SW,        /* 设置TEC4开关 0x55：关TEC4；0xAA：开TEC4。上电默认开 */

    P_S_TEC3_PARA = 0x9A, /* 设置第3路TEC 工作温度、限压、模式 设置TEC1参数，共2个参数，每个参数2个字节：1、工作温度（150～650，代表15.0℃～65.0℃）2、限压 */
    P_S_TEC4_PARA,        /* 设置第4路TEC 工作温度、限压、模式 设置TEC1参数，共2个参数，每个参数2个字节：1、工作温度（150～650，代表15.0℃～65.0℃）2、限压 */

    P_G_LD2_S_Cur = 0xB3, /* 查询LD电流设定值通道2 */
    P_G_Q_SW = 0x54,      /* todo 查询Q脉冲设定值 */

    P_G_TEC3_SW = 0xB5, /* 查询TEC3开关 */
    P_G_TEC4_SW,        /* 查询TEC4开关 */

    P_G_TEC3_PARA = 0xB7, /* 查询第3路TEC：温度、限压、模式设定值 */
    P_G_TEC4_PARA,        /* 查询第4路TEC：温度、限压、模式设定值 */

    P_G_LD2_M_Cur = 0xBA, /* 查询LD电流检测值通道2 */

    P_G_L2_M_V = 0xBB, /* 查询负载电压检测值通道2 */

    P_G_TEC3_M_TEMP = 0xBF, /* 查询第3路检测温度检测值 */
    P_G_TEC3_M_PW = 0xE0,   /* 查询第3路TEC输出功率检测值 */
    P_G_TEC4_M_TEMP,        /* 查询第4路检测温度检测值 */
    P_G_TEC4_M_PW,          /* 查询第4路TEC输出功率检测值 */
    P_G_TEC1_M_Cur,         /* 查询第1路TEC输出电流检测值 */
    P_G_TEC2_M_Cur,         /* 查询第2路TEC输出电流检测值 */
    P_G_TEC3_M_Cur,         /* 查询第3路TEC输出电流检测值 */
    P_G_TEC4_M_Cur,         /* 查询第4路TEC输出电流检测值 */

    ////////////////////////////////////////////////
    P_G_UPGRADE = 0x77,   /* 查询升级程序状态 */
    P_S_PulseType = 0x78, /* 设置脉冲类型 */
    P_G_PulseType = 0x79, /* 查询脉冲类型 */
    P_G_LD1_SW = 0x7A,    /* 查询LD1开关 */
    P_G_LD2_SW = 0x7B,    /* 查询LD2开关 */

    P_S_LD1_Vol = 0xC0, /* 设置LD1电压值 206～338代表20.6V～33.8V */
    P_S_LD2_Vol,        /* 设置LD2电压值 206～338代表20.6V～33.8V */
    P_S_LD1_HOC,        /* 设置LD1硬件过流  10～150代表1.0A～15.0A*/
    P_S_LD2_HOC,        /* 设置LD1硬件过流  10～150代表1.0A～15.0A*/
    P_S_LD1_SKB,        /* 设置LD1电流设定值标定参数 */
    P_S_LD2_SKB,        /* 设置LD2电流设定值标定参数 */
    P_S_LD1_MKB,        /* 设置LD1电流测量值标定参数 */
    P_S_LD2_MKB,        /* 设置LD2电流测量值标定参数 */
    P_S_PD_MKB,         /* 设置外部输入电平1（PD）（光功率检测值）检测值标定参数 */
    P_S_TEC1_PID,       /* 设置TEC1的PID参数 */
    P_S_TEC2_PID,       /* 设置TEC2的PID参数 */
    P_S_TEC3_PID,       /* 设置TEC3的PID参数 */
    P_S_TEC4_PID,       /* 设置TEC4的PID参数 */
    P_S_LCM,            /* 设置液冷模块参数 */

    P_G_LD1_Vol = 0xD0, /* 查询LD1电压值 206～338代表20.6V～33.8V */
    P_G_LD2_Vol,        /* 查询LD2电压值 206～338代表20.6V～33.8V */
    P_G_LD1_HOC,        /* 查询LD1硬件过流  10～150代表1.0A～15.0A*/
    P_G_LD2_HOC,        /* 查询LD1硬件过流  10～150代表1.0A～15.0A*/
    P_G_LD1_SKB,        /* 查询LD1电流设定值标定参数 */
    P_G_LD2_SKB,        /* 查询LD2电流设定值标定参数 */
    P_G_LD1_MKB,        /* 查询LD1电流测量值标定参数 */
    P_G_LD2_MKB,        /* 查询LD2电流测量值标定参数 */
    P_G_PD_MKB,         /* 查询外部输入电平1（PD）（光功率检测值）检测值标定参数 */
    P_G_TEC1_PID,       /* 查询TEC1的PID参数 */
    P_G_TEC2_PID,       /* 查询TEC2的PID参数 */
    P_G_TEC3_PID,       /* 查询TEC3的PID参数 */
    P_G_TEC4_PID,       /* 查询TEC4的PID参数 */
    P_G_S_LCM,          /* 查询液冷模块设置参数 */
    P_G_M_LCM,          /* 查询液冷模块反馈参数 */

    P_SAVE = 0x10,       /* 参数保存 */
    P_Clear_Err,         /* 清空错误 */
    P_S_Version,         /* 设置版本信息 */
    P_S_BOOTMODE,        /* 设置BOOT模式 */
    P_G_BOOTMODE = 0x14, /* 查询BOOT模式 */
};

enum
{

    M_LD1_S_Cur = 0xA0,   /* 查询LD电流设定值通道1 */
    M_LD2_S_Cur = 0xF0,   /* 查询LD电流设定值通道2 */
                          // M_Cur,                /* 查询电流设定值 */
    M_DFLT_V = 0xA1,      /* 查询初始电压设定值 */
    M_INTER_NOR_FREQ,     /* 查询内触发频率设定值 */
    M_PULSE_WIDTH,        /* 查询脉宽设定值 */
    M_Q_DELAY,            /* 查询调Ｑ延时设定值 */
    M_TRG_TYPE,           /* 查询触发模式 */
    M_LD1_SW,             /* LD1开关 */
    M_LD2_SW = 0xF6,      /* LD2开关 */
    M_Q_SW = 0xA7,        /* todo 查询Q脉冲设定值 */
    M_TEC1_SW,            /* 查询TEC1开关 */
    M_TEC2_SW,            /* 查询TEC2开关 */
    M_TEC3_SW = 0xF8,     /* 查询TEC3开关 */
    M_TEC4_SW,            /* 查询TEC4开关 */
    M_TEC1_PARA = 0xAA,   /* 查询第1路TEC：温度、限压、模式设定值 */
    M_TEC2_PARA,          /* 查询第2路TEC：温度、限压、模式设定值 */
    M_TEC3_PARA = 0xFA,   /* 查询第3路TEC：温度、限压、模式设定值 */
    M_TEC4_PARA,          /* 查询第4路TEC：温度、限压、模式设定值 */
    M_PULSE_PARA = 0xAC,  /* 查询子脉冲个数及脉冲间隔设定值 */
    M_LD1_M_Cur,          /* 查询LD电流检测值通道1 */
    M_LD2_M_Cur = 0xFD,   /* 查询LD电流检测值通道2 */
    M_L1_M_V = 0xAE,      /* 查询负载电压检测值通道1 */
    M_L2_M_V = 0xEE,      /* 查询负载电压检测值通道2 */
    M_PWR_TEMP = 0xAF,    /* 查询电源温度检测值 */
    M_OUT_PD = 0xC0,      /* 查询外部输入电平1（PD）检测值 */
    M_OUT_TEMP,           /* 查询外部输入电平2（温度）检测值 */
    M_TEC1_M_TEMP,        /* 查询第1路检测温度检测值 */
    M_TEC1_M_PW,          /* 查询第1路TEC输出功率检测值 */
    M_TEC2_M_TEMP,        /* 查询第2路检测温度检测值 */
    M_TEC2_M_PW,          /* 查询第2路TEC输出功率检测值 */
    M_TEC3_M_TEMP = 0x12, /* 查询第3路检测温度检测值 */
    M_TEC3_M_PW,          /* 查询第3路TEC输出功率检测值 */
    M_TEC4_M_TEMP,        /* 查询第4路检测温度检测值 */
    M_TEC4_M_PW,          /* 查询第4路TEC输出功率检测值 */
    M_TEC1_M_Cur,         /* 查询第1路TEC输出电流检测值 */
    M_TEC2_M_Cur,         /* 查询第2路TEC输出电流检测值 */
    M_TEC3_M_Cur,         /* 查询第3路TEC输出电流检测值 */
    M_TEC4_M_Cur,         /* 查询第4路TEC输出电流检测值 */

    M_WRK_STA = 0xC6, /* 查询工作状态 */
    M_ALL_SET,        /* 查询所有设定参数 */
    M_ALL_M = 0xCF,   /* 查询所有检测参数 */
    M_UPGRADE = 0xD0, /* 在线程序升级 */
    M_PulseType,      /* 查询脉冲类型 */
    ////////////////////////////////////////////////
    M_LD1_Vol = 0x20, /* 查询LD1电压值 206～338代表20.6V～33.8V */
    M_LD2_Vol,        /* 查询LD2电压值 206～338代表20.6V～33.8V */
    M_LD1_HOC,        /* 查询LD1硬件过流  10～150代表1.0A～15.0A*/
    M_LD2_HOC,        /* 查询LD1硬件过流  10～150代表1.0A～15.0A*/
    M_LD1_SKB,        /* 查询LD1电流设定值标定参数 */
    M_LD2_SKB,        /* 查询LD2电流设定值标定参数 */
    M_LD1_MKB,        /* 查询LD1电流测量值标定参数 */
    M_LD2_MKB,        /* 查询LD2电流测量值标定参数 */
    M_PD_MKB,         /* 查询外部输入电平1（PD）（光功率检测值）检测值标定参数 */
    M_TEC1_PID,       /* 查询TEC1的PID参数 */
    M_TEC2_PID,       /* 查询TEC2的PID参数 */
    M_TEC3_PID,       /* 查询TEC3的PID参数 */
    M_TEC4_PID,       /* 查询TEC4的PID参数 */
    M_S_LCM,          /* 查询液冷模块设置参数 */
    M_M_LCM,          /* 查询液冷模块反馈参数 */

    M_SAVE = 0xe0,           /* 参数保存 反馈 */ 
    M_BOOT_MODE = 0xe4,      /* BOOT模式 反馈 */

};

#pragma pack(1)
typedef struct
{
    uint16_t head;
    uint8_t sendID;
    uint8_t revID;
    uint8_t cmd;
    uint8_t len;
    uint8_t crc;
    uint16_t tail;
} min_cmd_t;
#pragma unpack()

static uint8_t sum_crc(uint8_t *data, uint8_t len)
{
    uint8_t sum = 0;
    for (size_t i = 0; i < len; i++)
    {
        sum += data[i];
    }
    return sum;
  
}

static uint8_t CheckUartReady()
{
    HAL_DMA_StateTypeDef res = HAL_DMA_GetState(huart1.hdmatx);
    uint8_t loop = 0;
    while (res != HAL_DMA_STATE_READY && loop < 50)
    {
        Delay_ms(1);
        loop++;
        res = HAL_DMA_GetState(huart1.hdmatx);
    }
    if (res != HAL_DMA_STATE_READY)
    {
        return 0;
    }
    return 1;
}

static void uart_send(uint8_t cmd, void *data, uint8_t dataLen)
{
    static uint8_t UartTxBuff[MAX_SIZE];
    min_cmd_t *s = UartTxBuff;
    s->head = CMD_HEAD;
    s->sendID = MCU_ID;
    s->revID = PC_ID;
    s->cmd = cmd;
    s->len = dataLen;
    if (dataLen > 0)
    {
        memcpy((UartTxBuff + 6), data, dataLen);
    }
    UartTxBuff[6 + dataLen] = sum_crc(data, dataLen);
    UartTxBuff[6 + dataLen + 1] = CMD_TAIL & 0xff;
    UartTxBuff[6 + dataLen + 2] = CMD_TAIL >> 8;
    uint8_t length = dataLen + sizeof(min_cmd_t);
    // HAL_UART_Transmit(&huart1, UartTxBuff, length, 0xfff);
    HAL_UART_Transmit_DMA(&huart1, UartTxBuff, length);
}

static void pc_send_ack(uint8_t cmd, void *data, uint8_t dataLen)
{
    if (CheckUartReady())
    {
        uart_send(cmd, data, dataLen);
    }
}


#define PC_ACK(cmd, data) pc_send_ack(cmd, &data, sizeof(data))

#define PC_ACK_INT16(cmd, data)                    \
    {                                              \
        int16_t data1 = (int16_t)round(data);      \
        pc_send_ack(cmd, &data1, sizeof(int16_t)); \
    }

#define PC_ACK_UINT16(cmd, data)                    \
    {                                               \
        uint16_t data1 = (uint16_t)round(data);     \
        pc_send_ack(cmd, &data1, sizeof(uint16_t)); \
    }




#define SET_PARAM_INT16(param) (param) = *(uint16_t *)data
#define SET_PARAM_INT8(param) (param) = *(uint8_t *)data

#define SET_SW_STA(param)                        \
    {                                            \
        uint8_t type = *(uint8_t *)data;         \
        if (type == WORK_OFF || type == WORK_ON) \
            param = type;                        \
    }

#define SET_PULSE_TYPE(param)                    \
    {                                            \
        uint8_t type = *(uint8_t *)data;         \
        if (type == WORK_OFF || type == WORK_ON) \
        {                                        \
            param = type;                        \
        }                                        \
    }

static void set_tec_param(uint8_t ch, tec_setparam_t *tec, uint8_t *data)
{
    tec_packet_t *p = data;
    if (p->Temp >= TEC_SET_MIN_TEMP && p->Temp <= TEC_SET_MAX_TEMP)
    {
        tec->Temp = p->Temp;
    }
    tec->MaxVol = p->V;
    if (tec->sw == WORK_ON )
    {
        TEC_RestStatus(ch);
    }
}

static void get_tec_param(uint8_t cmd, tec_setparam_t *tec)
{
    tec_packet_t p;
    p.Temp = tec->Temp;
    p.V = tec->MaxVol;
    PC_ACK(cmd, p);
}

static void get_all_set_param(uint8_t cmd, set_param_t *data)
{
#pragma pack(1)
    struct
    {
        uint16_t LD1_Cur;    // 单位 0.1A
        uint16_t LD2_Cur;    // 单位 0.1A
        uint16_t DFLT_V;     // 初始电压 单位 1V
        uint16_t Nor_Freq;   // 内部触发定频脉冲 频率 单位Hz
        uint16_t PulseWidth; // 脉冲宽度 单位us
        uint16_t Q_DELAY;    // 调Q延时 单位us
        tec_packet_t TEC[4];
        Pulse_param_packet_t Pulse_para;
    } p;
#pragma unpack()
    p.LD1_Cur = data->ld[0].Cur;
    p.LD2_Cur = data->ld[1].Cur;
    /* p.DFLT_V = data->DFLT_V; */
    p.Nor_Freq = data->Pulse_para.Nor_Freq;
    p.PulseWidth = data->Pulse_para.Width;
    p.Q_DELAY = data->T_Q.delay;
    for (size_t i = 0; i < 4; i++)
    {
        p.TEC[i].Temp = data->tec[i].Temp;
        p.TEC[i].V = data->tec[i].MaxVol;
    }
    p.Pulse_para.Num = data->Pulse_para.Num;
    for (size_t i = 0; i < p.Pulse_para.Num; i++)
    {
        p.Pulse_para.Interval[i] = data->Pulse_para.Interval[i];
    }
    PC_ACK(cmd, p);
}

static void get_pulse_param(uint8_t cmd, set_param_t *data)
{
    Pulse_param_packet_t p;
    p.Num = data->Pulse_para.Num;
    for (size_t i = 0; i < p.Num; i++)
    {
        p.Interval[i] = data->Pulse_para.Interval[i];
    }
    PC_ACK(cmd, p);
}

static void get_all_measure_param(uint8_t cmd, measure_param_t *data)
{
#pragma pack(1)
    struct
    {
        uint16_t LD1_Cur;
        uint16_t LD2_Cur;
        uint16_t LD1_V;
        uint16_t LD2_V;
        int16_t PWR_Temp;
        uint16_t OUT_PD;
        int16_t OUT_TEMP;
        struct
        {
            int16_t Temp;
            int16_t Power;
            int16_t Cur;
        } TEC[4];
        uint16_t sys_vol;
        uint16_t sys_cur;
        Work_Status_t Work_Status;
        char Version[4];
    } p;
#pragma unpack()
    p.LD1_Cur = round(data->ld[0].Cur * 10);
    p.LD2_Cur = round(data->ld[1].Cur * 10);
    p.LD1_V = round(data->ld[0].Vol * 10);
    p.LD2_V = round(data->ld[1].Vol * 10);
    p.PWR_Temp = round(data->PWR_Temp * 10);

    p.OUT_PD = round(data->out.PD * 10);
    p.OUT_TEMP = round(data->out.Temp * 10);

    for (size_t i = 0; i < 4; i++)
    {
        //p.TEC[i].Temp = round(data->tec[i].Temp * 10);
        p.TEC[i].Temp = round(data->tec[i].Temp_f * 10);
        p.TEC[i].Power = round(data->tec[i].Power * 10);
        p.TEC[i].Cur = round(data->tec[i].Cur * 10);
        p.Version[i] = Version2[i];
    }
    p.sys_vol = round(data->sys.Vol * 10);
    p.sys_cur = round(data->sys.Cur * 10);
    p.Work_Status = Work_Status;
    PC_ACK(cmd, p);
   
}

static void set_pulse_param(uint8_t *data)
{
    Pulse_param_packet_t *p = data;
    uint16_t sum_pulse_width=0;
    // todo 判断
    if (p->Num > PULSE_MAX_SECTION)
    {
        return;
    }
    for (size_t i = 0; i < p->Num; i++)
    {
        sum_pulse_width += p->Interval[i];
        if (p->Interval[i] > 660 || p->Interval[i] < 240)
        {
            return;
        }
    }
    if (sum_pulse_width > 5000)
    {
        return;
    }
    set_param.Pulse_para.Num = p->Num;
    for (size_t i = 0; i < p->Num; i++)
    {
        set_param.Pulse_para.Interval[i] = p->Interval[i];
    }
    CalcPulse_SPWMParam();
}

static void set_kb_param(cali_coef_t *coef, uint8_t *data)
{
    kb_packet_t *p = data;
    coef->k = p->k / 100.0;
    coef->b = p->b / 100.0;
}

static void get_kb_param(uint8_t cmd, cali_coef_t *coef)
{
    kb_packet_t p;
    p.k = round(coef->k * 100) ;
    p.b = round(coef->b * 100);
    PC_ACK(cmd, p);
}

static void set_pid_param(PID_Controller *pid, uint8_t *data)
{
    pid_packet_t *p = data;
    pid->Kp = p->p / 100.0;
    pid->Ki = p->i / 100.0;
    pid->Kd = p->d / 100.0;
}

static void get_pid_param(uint8_t cmd, PID_Controller *pid)
{
    pid_packet_t p;
    p.p = round(pid->Kp * 100);
    p.i = round(pid->Ki * 100);
    p.d = round(pid->Kd * 100);
    PC_ACK(cmd, p);
}

static void set_LD_HOC_param(uint8_t ch, uint8_t *data)
{
    set_param.ld[ch].HOC = *(uint16_t *)data;
    if (set_param.ld[ch].HOC > LD_MAX_CUR * 10)
    {
        set_param.ld[ch].HOC = LD_MAX_CUR * 10;
    }
    SET_LD_MAX_Curr(ch, set_param.ld[ch].HOC / 10.0);
}
static void set_LD_Vol_param(uint8_t ch, uint8_t *data)
{
    set_param.ld[ch].Vol = *(uint16_t *)data;
    if (set_param.ld[ch].Vol > LD_MAX_VOL * 10)
    {
        set_param.ld[ch].Vol = LD_MAX_VOL * 10;
    }
    else if (set_param.ld[ch].Vol < LD_MIN_VOL * 10)
    {
        set_param.ld[ch].Vol = LD_MIN_VOL * 10;
    }
    SET_LD_Vol(ch, set_param.ld[ch].Vol / 10.0);
}

static void set_LCM_param(uint8_t *data)
{
    S_LCM_t p = *(S_LCM_t *)data;


    if (p.PwrEn != WORK_ON)
    {
        p.PwrEn = WORK_OFF;
        HAL_GPIO_WritePin(LCVG_ONOFF_GPIO_Port, LCVG_ONOFF_Pin, 0);
    }
    else
    {
        HAL_GPIO_WritePin(LCVG_ONOFF_GPIO_Port, LCVG_ONOFF_Pin, 1);
    }
    if (p.Fan != WORK_ON)
    {
        p.Fan = WORK_OFF;
        STOP_FAN;
        //    STOP_FAN_PWM;
    }
    else
    {
        // START_FAN_PWM;
        // SET_FAN_PWM_DutyCycle(100);
        START_FAN;
    }

    if (p.MotorEn != WORK_ON)
    {
        p.MotorEn = WORK_OFF;
    }


    if (p.MotorSpeed > 30000)
    {
        p.MotorSpeed = 30000;
    }
    if (p.PwrLimit > 250)
    {
        p.PwrLimit = 250;
    }
    set_param.LCM = p;

    pump_set_param.state = (p.MotorEn == WORK_ON ? 1 : 0);
    pump_set_param.PwrLimit = p.PwrLimit;
    pump_set_param.MotorSpeed = p.MotorSpeed;
    //Delay_ms(20);
    pump_setting(&pump_set_param);
}

static void ClearErrs()
{
    set_param.ld[0].ErrStatus.value = 0;
    set_param.ld[1].ErrStatus.value = 0;
    for (size_t i = 0; i < 4; i++)
    {
        set_param.tec[i].ErrStatus.value = 0;
    }
    memset(&Work_Status,0,sizeof(Work_Status));
    upgrade_ini();
}

static void save_param(uint8_t cmd)
{
    uint8_t p = 0;
    p = savePara();
    PC_ACK(cmd, p);
}

static void set_Version(uint8_t *p)
{
   memcpy(Version2,p,4);
   saveBitInfo();
}

static void set_pulse_type(uint8_t data)
{
    if (set_param.ld[0].sw == WORK_ON || set_param.ld[1].sw == WORK_ON)
    {
        return;
    }
    if (data == WORK_OFF || data == WORK_ON)
        set_param.Pulse_Type = data;
}

static void set_TRG_type(uint8_t data)
{
    if (set_param.ld[0].sw == WORK_ON || set_param.ld[1].sw == WORK_ON)
    {
        return;
    }
    if (data == WORK_OFF || data == WORK_ON)
        set_param.TRG_Type = data;
}



static void ack_boot_mode()
{
    uint8_t status = get_bootmode();
    PC_ACK(M_BOOT_MODE, status);
}

static void exec_commands(uint8_t cmd, uint8_t *data, size_t data_len)
{
     
   // *(float *)data
   switch (cmd)
   {
    
   case P_S_LD1_S_Cur:              SET_PARAM_INT16(set_param.ld[0].Cur);                                       break;
   case P_S_LD2_S_Cur:              SET_PARAM_INT16(set_param.ld[1].Cur);                                       break;
   case P_S_DFLT_V:                 /* SET_PARAM_INT16(set_param.DFLT_V);  */                             break;
   case P_S_INTER_NOR_FREQ:         SET_PARAM_INT16(set_param.Pulse_para.Nor_Freq);CalcPulse_NORParam();                             break;
   case P_S_PULSE_WIDTH:            SET_PARAM_INT16(set_param.Pulse_para.Width);CalcPulse_SPWMParam();CalcPulse_NORParam();                                break;
   case P_S_Q_DELAY:                SET_PARAM_INT16(set_param.T_Q.delay);CalcPulse_SPWMParam();CalcPulse_NORParam();                                       break;
   case P_S_TRG_TYPE:               set_TRG_type(*(uint8_t *)data);                                         break;
   case P_S_PulseType:              set_pulse_type(*(uint8_t *)data);                                       break;
   case P_S_LD1_SW:                 SET_SW_STA(set_param.ld[0].sw);                                             break;
   case P_S_LD2_SW:                 SET_SW_STA(set_param.ld[1].sw);                                             break;
   case P_S_Q_SW:                   SET_SW_STA(set_param.T_Q.sw);                                               break;
   case P_S_TEC1_SW:                SET_SW_STA(set_param.tec[0].sw);                                            break;
   case P_S_TEC2_SW:                SET_SW_STA(set_param.tec[1].sw);                                            break;
   case P_S_TEC3_SW:                SET_SW_STA(set_param.tec[2].sw);                                            break;
   case P_S_TEC4_SW:                SET_SW_STA(set_param.tec[3].sw);                                            break;
   case P_S_TEC1_PARA:              set_tec_param(0,&set_param.tec[0], data);                                     break;
   case P_S_TEC2_PARA:              set_tec_param(1,&set_param.tec[1], data);                                     break;
   case P_S_TEC3_PARA:              set_tec_param(2,&set_param.tec[2], data);                                     break;
   case P_S_TEC4_PARA:              set_tec_param(3,&set_param.tec[3], data);                                     break;
   case P_S_PULSE_PARA:             set_pulse_param(data);                                                      break;
   case P_G_Cur:       break;
   case P_G_DFLT_V:                 /* PC_ACK(M_DFLT_V, set_param.DFLT_V); */                                   break;
   case P_G_INTER_NOR_FREQ:         PC_ACK(M_INTER_NOR_FREQ, set_param.Pulse_para.Nor_Freq);                    break;
   case P_G_PULSE_WIDTH:            PC_ACK(M_PULSE_WIDTH,set_param.Pulse_para.Width);                           break;
   case P_G_Q_DELAY:                PC_ACK(M_Q_DELAY, set_param.T_Q.delay);                                     break;
   case P_G_TRG_TYPE:               PC_ACK(M_TRG_TYPE, set_param.TRG_Type);                                     break;
   case P_G_LD1_S_Cur:              PC_ACK(M_LD1_S_Cur, set_param.ld[0].Cur);                                   break;
   case P_G_LD2_S_Cur:              PC_ACK(M_LD2_S_Cur, set_param.ld[1].Cur);                                   break;
   case P_G_Q_SW:                   PC_ACK(M_Q_SW, set_param.T_Q.sw);                                           break;
   case P_G_TEC1_SW:                PC_ACK(M_TEC1_SW, set_param.tec[0].sw);                                     break;
   case P_G_TEC2_SW:                PC_ACK(M_TEC2_SW, set_param.tec[1].sw);                                     break;
   case P_G_TEC3_SW:                PC_ACK(M_TEC3_SW, set_param.tec[2].sw);                                     break;
   case P_G_TEC4_SW:                PC_ACK(M_TEC4_SW, set_param.tec[3].sw);                                     break;
   case P_G_TEC1_PARA:              get_tec_param(M_TEC1_PARA, &set_param.tec[0]);                              break;
   case P_G_TEC2_PARA:              get_tec_param(M_TEC2_PARA, &set_param.tec[1]);                              break;
   case P_G_TEC3_PARA:              get_tec_param(M_TEC3_PARA, &set_param.tec[2]);                              break;
   case P_G_TEC4_PARA:              get_tec_param(M_TEC4_PARA, &set_param.tec[3]);                              break;
   case P_G_PULSE_PARA:             get_pulse_param(M_PULSE_PARA, &set_param);                                  break;
   case P_G_LD1_M_Cur:              PC_ACK_UINT16(M_LD1_M_Cur, measure_param.ld[0].Cur * 10);                   break;
   case P_G_LD2_M_Cur:              PC_ACK_UINT16(M_LD2_M_Cur, measure_param.ld[1].Cur * 10);                   break;
   case P_G_LD1_M_V:                 PC_ACK_UINT16(M_L1_M_V, measure_param.ld[0].Vol * 10);                      break;
   case P_G_L2_M_V:                 PC_ACK_UINT16(M_L2_M_V, measure_param.ld[1].Vol * 10);                      break;
   case P_G_PWR_TEMP:               PC_ACK_INT16(M_PWR_TEMP, measure_param.PWR_Temp * 10);                      break;
   case P_G_OUT_PD:                 PC_ACK_INT16(M_OUT_PD, measure_param.out.PD * 10);                          break;
   case P_G_OUT_TEMP:               PC_ACK_INT16(M_OUT_TEMP, measure_param.out.Temp * 10);                      break;
   case P_G_TEC1_M_TEMP:            PC_ACK_INT16(M_TEC1_M_TEMP, measure_param.tec[0].Temp * 10);                break;
   case P_G_TEC1_M_PW:              PC_ACK_INT16(M_TEC1_M_PW, measure_param.tec[0].Power * 10);                 break;
   case P_G_TEC2_M_TEMP:            PC_ACK_INT16(M_TEC2_M_TEMP, measure_param.tec[1].Temp * 10);                break;
   case P_G_TEC2_M_PW:              PC_ACK_INT16(M_TEC2_M_PW, measure_param.tec[1].Power * 10);                 break;
   case P_G_TEC3_M_TEMP:            PC_ACK_INT16(M_TEC3_M_TEMP, measure_param.tec[2].Temp * 10);                break;
   case P_G_TEC3_M_PW:              PC_ACK_INT16(M_TEC3_M_PW, measure_param.tec[2].Power * 10);                 break;
   case P_G_TEC4_M_TEMP:            PC_ACK_INT16(M_TEC4_M_TEMP, measure_param.tec[3].Temp * 10);                break;
   case P_G_TEC4_M_PW:              PC_ACK_INT16(M_TEC4_M_PW, measure_param.tec[3].Power * 10);                 break;
   case P_G_TEC1_M_Cur:             PC_ACK_INT16(M_TEC1_M_Cur, measure_param.tec[0].Cur * 10);                  break;
   case P_G_TEC2_M_Cur:             PC_ACK_INT16(M_TEC2_M_Cur, measure_param.tec[1].Cur * 10);                  break;
   case P_G_TEC3_M_Cur:             PC_ACK_INT16(M_TEC3_M_Cur, measure_param.tec[2].Cur * 10);                  break;
   case P_G_TEC4_M_Cur:             PC_ACK_INT16(M_TEC4_M_Cur, measure_param.tec[3].Cur * 10);                  break;
   
   
   case P_G_WRK_STA:                PC_ACK(M_WRK_STA, Work_Status);                                             break;
   case P_G_ALL_SET:                get_all_set_param(M_ALL_SET, &set_param);                                   break;
   case P_G_ALL_M:                  get_all_measure_param(M_ALL_M, &measure_param);                             break; 

   case P_G_PulseType:              PC_ACK(M_PulseType, set_param.Pulse_Type);                                  break;
   case P_G_LD1_SW:                 PC_ACK(M_LD1_SW, set_param.ld[0].sw);                                       break;
   case P_G_LD2_SW:                 PC_ACK(M_LD2_SW, set_param.ld[1].sw);                                       break;

   case P_S_LD1_Vol:                set_LD_Vol_param(LD_CH_1, data);                                            break;
   case P_S_LD2_Vol:                set_LD_Vol_param(LD_CH_2, data);                                            break;
   case P_S_LD1_HOC:                set_LD_HOC_param(LD_CH_1, data);                                            break;
   case P_S_LD2_HOC:                set_LD_HOC_param(LD_CH_2, data);                                            break;
   case P_S_LD1_SKB:                set_kb_param(&set_param.ld[0].calib_set, data);                             break;
   case P_S_LD2_SKB:                set_kb_param(&set_param.ld[1].calib_set, data);                             break;
   case P_S_LD1_MKB:                set_kb_param(&set_param.ld[0].calib_measure, data);                         break;
   case P_S_LD2_MKB:                set_kb_param(&set_param.ld[1].calib_measure, data);                         break;
   case P_S_PD_MKB:                 set_kb_param(&set_param.PD_calib, data);                                    break;
   case P_S_TEC1_PID:               set_pid_param(&set_param.tec[0].PID, data);                                 break;
   case P_S_TEC2_PID:               set_pid_param(&set_param.tec[1].PID, data);                                 break;
   case P_S_TEC3_PID:               set_pid_param(&set_param.tec[2].PID, data);                                 break;
   case P_S_TEC4_PID:               set_pid_param(&set_param.tec[3].PID, data);                                 break;
   case P_S_LCM:                    set_LCM_param(data);                                                        break;

   case P_G_LD1_Vol:                PC_ACK(M_LD1_Vol, set_param.ld[0].Vol);                                     break;
   case P_G_LD2_Vol:                PC_ACK(M_LD2_Vol, set_param.ld[1].Vol);                                     break;
   case P_G_LD1_HOC:                PC_ACK(M_LD1_HOC, set_param.ld[0].HOC);                                     break;
   case P_G_LD2_HOC:                PC_ACK(M_LD2_HOC, set_param.ld[1].HOC);                                     break;
   case P_G_LD1_SKB:                get_kb_param(M_LD1_SKB, &set_param.ld[0].calib_set);                        break;
   case P_G_LD2_SKB:                get_kb_param(M_LD2_SKB, &set_param.ld[1].calib_set);                        break;
   case P_G_LD1_MKB:                get_kb_param(M_LD1_MKB, &set_param.ld[0].calib_measure);                    break;
   case P_G_LD2_MKB:                get_kb_param(M_LD2_MKB, &set_param.ld[1].calib_measure);                    break;
   case P_G_PD_MKB:                 get_kb_param(M_PD_MKB, &set_param.PD_calib);                                break;
   case P_G_TEC1_PID:               get_pid_param(M_TEC1_PID, &set_param.tec[0].PID);                           break;
   case P_G_TEC2_PID:               get_pid_param(M_TEC2_PID, &set_param.tec[1].PID);                           break;
   case P_G_TEC3_PID:               get_pid_param(M_TEC3_PID, &set_param.tec[2].PID);                           break;
   case P_G_TEC4_PID:               get_pid_param(M_TEC4_PID, &set_param.tec[3].PID);                           break;
   case P_G_S_LCM:                  PC_ACK(M_S_LCM, set_param.LCM);                                             break;
   case P_G_M_LCM:                  PC_ACK(M_M_LCM, measure_param.LCM);                                         break;

   case P_SAVE:                     save_param(M_SAVE);                                                         break;
   case P_Clear_Err:                ClearErrs();                                                                break;
   case P_S_Version:                set_Version(data);                                                          break; 
   case P_S_BOOTMODE:               set_boot_bootmode(*(uint8_t *)data);ack_boot_mode();                        break;
   case P_G_BOOTMODE:               ack_boot_mode();                                                            break;
   default:
       break;
   }
   
}

void pc_parse_and_execute_command2()
{
    uart_para_t *uart_para = &uart2_para;
    //拷贝数据后 再进行处理？
    if (uart_para->pktcplt && uart_para->tail >= sizeof(min_cmd_t))
    {
        // Disable_UART2_Receive();
        uint16_t cmd_pos = 0;
        for (size_t i = 0; i < uart_para->tail; i++)
        {
            min_cmd_t *data = (min_cmd_t *)(uart_para->rxbuf + i);
            if (data->head == CMD_HEAD &&                                                                         /* 帧头校验 */
                data->sendID == PC_ID &&                                                                          /* 发送id校验 */
                data->revID == MCU_ID &&                                                                          /* 接收id校验 */
                uart_para->tail - cmd_pos >= sizeof(min_cmd_t) + data->len &&                                 /* 长度满足要求 */
                sum_crc(uart_para->rxbuf + i + 6, data->len) == *(uint8_t *)(uart_para->rxbuf + i + 6 + data->len) && /* 数据和校验 */
                *(uint16_t *)(uart_para->rxbuf + i + 7 + data->len) == CMD_TAIL                                     /* 帧尾校验 */
            )
            {
                exec_commands(data->cmd, (uint8_t *)data + 6, data->len);
                i += sizeof(min_cmd_t) + data->len - 1;
                cmd_pos = i + 1;
            }
        }

        if (cmd_pos < uart_para->tail)
        {
            memcpy(uart_para->rxbuf, uart_para->rxbuf + cmd_pos, uart_para->tail - cmd_pos);
            uart_para->tail -= (cmd_pos);
        }
        else
        {
            uart_para->tail = 0;
        }
        if (uart_para->tail == CLI_RX_BUFF)
        {
            uart_para->tail = 0;
        }
        uart_para->pktcplt = 0;
        // Enable_UART2_Receive();
    }

    // UART2_Check();
}





