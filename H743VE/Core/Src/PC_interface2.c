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
#include "protocol_comm.h"
#include "math.h"

#define PC_ACK_BK send_ack(huart, cmd, data, data_len)

// char Version[] = "X004_001";
// uint64_t BuildTime = 202509112205;

// static char Version2[] = {0, 0, 0, 3}; // V0.0.0.2

typedef struct
{
    uint16_t Temp; // 设置温度 单位 0.1℃
    uint16_t V;    // 最高电压 单位 0.1V
} tec_packet_t;

#pragma pack(1)
typedef struct
{
    uint8_t PWR_OT : 1;      // 主板过温
    uint8_t PWR_ERR : 1;     // 主板故障
    uint8_t LD1_UC : 1;      // LD1驱动欠流
    uint8_t LD1_OC : 1;      // LD1驱动过流
    uint8_t EPPROM_ERR : 1;  // 参数存储错误
    uint8_t NTC1_ERR : 1;    // 热敏电阻1异常
    uint8_t NTC2_ERR : 1;    // 热敏电阻2异常
    uint8_t remain1 : 1;     //
    uint8_t TEC1_SW : 1;     // TEC1 开关 1：开启; 0：关闭
    uint8_t TEC2_SW : 1;     // TEC2 开关 1：开启; 0：关闭
    uint8_t Q_SW : 1;        // 调Q 开关 1：开启; 0：关闭
    uint8_t LD1_SW : 1;      // LD1 开关 1：开启; 0：关闭
    uint8_t TRQ_Type : 1;    // 触发状态 1：外触发; 0：内触发
    uint8_t NTC3_ERR : 1;    // 热敏电阻3异常
    uint8_t NTC4_ERR : 1;    // 热敏电阻4异常
    uint8_t LD2_UC : 1;      // LD2驱动欠流
    uint8_t LD2_OC : 1;      // LD2驱动过流
    uint8_t TEC3_SW : 1;     // TEC3 开关 1：开启; 0：关闭
    uint8_t TEC4_SW : 1;     // TEC4 开关 1：开启; 0：关闭
    uint8_t LD2_SW : 1;      // LD2 开关 1：开启; 0：关闭
    uint8_t LCM_Fan : 1;     // LCM风扇 1：开启; 0：关闭
    uint8_t LCM_MotorEn : 1; // LCM电机开关 1：使能; 0：关闭
    uint8_t Pulse_Type : 1;  // 脉冲类型 1：定频; 0：变频
    uint8_t remain : 1; //
} Work_Status2_t;
#pragma unpack()

Work_Status2_t Work_Status2;

#pragma pack(1)
typedef struct
{
    uint16_t LD1_Cur;
    uint16_t LD1_V;
    int16_t PWR_Temp;
    uint16_t OUT_PD;
    int16_t OUT_TEMP;
    int16_t TEC1_Temp;
    int16_t TEC1_Power;
    int16_t TEC2_Temp;
    int16_t TEC2_Power;
    Work_Status2_t Work_Status;
    char Version[4];
    uint16_t LD2_Cur;
    uint16_t LD2_V;
    int16_t TEC3_Temp;
    int16_t TEC3_Power;
    int16_t TEC4_Temp;
    int16_t TEC4_Power;
    int16_t LCM_Temp;
    uint16_t LCM_Set_MotorSpeed;
    uint16_t LCM_M_MotorSpeed;
    uint16_t LCM_M_state;
} measure_packet_t;
#pragma unpack()

#pragma pack(1)
typedef struct
{
    uint16_t LD1_Cur;    // 单位 0.1A
    uint16_t PulseWidth; // 脉冲宽度 单位us
    uint16_t Q_DELAY;    // 调Q延时 单位us
    int16_t TEC1_Temp;   // TEC1温度设定值 单位0.1℃
    int16_t TEC1_V;      // TEC1限压设定值 单位0.1V
    int16_t TEC2_Temp;   // TEC2温度设定值 单位0.1℃
    int16_t TEC2_V;      // TEC2限压设定值 单位0.1V
    Pulse_param_packet_t Pulse_para;
    uint16_t LD2_Cur;  // 单位 0.1A
    int16_t TEC3_Temp; // TEC3温度设定值 单位0.1℃
    int16_t TEC3_V;    // TEC3限压设定值 单位0.1V
    int16_t TEC4_Temp; // TEC4温度设定值 单位0.1℃
    int16_t TEC4_V;    // TEC4限压设定值 单位0.1V

} set_packet_t;
#pragma unpack()

enum
{
    P_S_LD1_S_Cur = 0x30,      /* 设置电流通道1 10～150代表1.0A～15.0A */
    P_S_PULSE_WIDTH = 0x33,    /* 设置脉宽 200us～240us */
    P_S_Q_DELAY = 0x34,        /* 设置调Ｑ延时 1us～300us */
    P_S_TRG_TYPE = 0x35,       /* 设置触发模式 0x55内触发；0xAA外触发 */
    P_S_LD1_SW = 0x36,         /* 设置LD电流开关通道1 0x55:关LD电流；0xAA：开LD电流。上电默认关 */
    P_S_Q_SW = 0x37,           /* 设置Q脉冲开关 0x55:关Ｑ脉冲；0xAA：开Ｑ脉冲。上电默认开 */
    P_S_TEC1_SW = 0x38,        /* 设置TEC1开关 0x55：关TEC1；0xAA：开TEC1。上电默认开 */
    P_S_TEC2_SW = 0x39,        /* 设置TEC2开关 0x55：关TEC2；0xAA：开TEC2。上电默认开 */
    P_S_TEC1_PARA = 0x3A,      /* 设置第1路TEC：工作温度、限压、模式 设置TEC1参数，共2个参数，每个参数2个字节：1、工作温度（150～650，代表15.0℃～65.0℃）2、限压 */
    P_S_TEC2_PARA = 0x3B,      /* 设置第2路TEC 工作温度、限压、模式 设置TEC2参数，共2个参数，每个参数2个字节：1、工作温度（150～650，代表15.0℃～65.0℃）2、限压 */
    P_S_PULSE_PARA = 0x3C,     /* 设置子脉冲参数，共12个参数，每个参数2个字节：1、子脉冲个数（取值范围1~12）2~11、脉冲间隔1（取值范围240~660）*/
    P_G_PULSE_WIDTH = 0x50,    /* 查询脉宽设定值 */
    P_G_Q_DELAY = 0x51,        /* 查询调Ｑ延时设定值 */
    P_G_TRG_TYPE = 0x52,       /* 查询触发模式 */
    P_G_LD1_S_Cur = 0x53,      /* 查询LD电流设定值通道1 */
    P_G_TEC1_SW = 0x55,        /* 查询TEC1开关 */
    P_G_TEC2_SW = 0x56,        /* 查询TEC2开关 */
    P_G_TEC1_PARA = 0x57,      /* 查询第1路TEC：温度、限压设定值 */
    P_G_TEC2_PARA = 0x58,      /* 查询第2路TEC：温度、限压设定值 */
    P_G_PULSE_PARA = 0x59,     /* 查询子脉冲个数及脉冲间隔设定值 */
    P_G_LD1_M_Cur = 0x5A,      /* 查询LD电流检测值通道1 */
    P_G_LD1_M_V = 0x5B,        /* 查询负载电压检测值通道1 */
    P_G_PWR_TEMP = 0x5C,       /* 查询电源温度检测值 */
    P_G_OUT_PD = 0x5D,         /* 查询外部输入电平1（PD）检测值 */
    P_G_OUT_TEMP = 0x5E,       /* 查询外部输入电平2（温度）检测值 */
    P_G_TEC1_M_TEMP = 0x5F,    /* 查询第1路检测温度检测值 */
    P_G_TEC1_M_PW = 0x70,      /* 查询第1路TEC输出功率检测值 */
    P_G_TEC2_M_TEMP = 0x71,    /* 查询第2路检测温度检测值 */
    P_G_TEC2_M_PW = 0x72,      /* 查询第2路TEC输出功率检测值 */
    P_G_WRK_STA = 0x73,        /* 查询工作状态 */
    P_G_ALL_SET = 0x74,        /* 查询所有设定参数 */
    P_G_ALL_M = 0x75,          /* 查询所有检测参数 */
    P_S_UPGRADE = 0x76,        /* 在线程序升级 */
    P_S_LD2_SW = 0xB6,         /* 设置LD电流开关通道2 0x55:关LD电流；0xAA：开LD电流。上电默认关 */
    P_G_LD2_S_Cur = 0x93,      /* 查询LD电流设定值通道2 */
    P_S_LD2_S_Cur = 0x90,      /* 设置电流通道2 10～150代表1.0A～15.0A */
    P_S_TEC3_SW = 0x98,        /* 设置TEC3开关 0x55：关TEC3；0xAA：开TEC3。上电默认开 */
    P_S_TEC4_SW = 0x99,        /* 设置TEC4开关 0x55：关TEC4；0xAA：开TEC4。上电默认开 */
    P_G_TEC3_SW = 0x95,        /* 查询TEC3开关 */
    P_G_TEC4_SW = 0x96,        /* 查询TEC4开关 */
    P_S_TEC3_PARA = 0x9A,      /* 设置第3路TEC 工作温度、限压、模式 设置TEC1参数，共2个参数，每个参数2个字节：1、工作温度（150～650，代表15.0℃～65.0℃）2、限压 */
    P_S_TEC4_PARA = 0x9B,      /* 设置第4路TEC 工作温度、限压、模式 设置TEC1参数，共2个参数，每个参数2个字节：1、工作温度（150～650，代表15.0℃～65.0℃）2、限压 */
    P_G_TEC3_PARA = 0xB7,      /* 查询第3路TEC：温度、限压设定值 */
    P_G_TEC4_PARA = 0xB8,      /* 查询第4路TEC：温度、限压设定值 */
    P_G_LD2_M_Cur = 0xBA,      /* 查询LD电流检测值通道2 */
    P_G_LD2_M_V = 0xBB,        /* 查询负载电压检测值通道2 */
    P_G_TEC3_M_TEMP = 0xBF,    /* 查询第3路检测温度检测值 */
    P_G_TEC4_M_TEMP = 0xE1,    /* 查询第4路检测温度检测值 */
    P_G_TEC3_M_PW = 0xB0,      /* 查询第3路TEC输出功率检测值 */
    P_G_TEC4_M_PW = 0xE2,      /* 查询第4路TEC输出功率检测值 */
    P_S_BOOTMODE = 0xE3,       /* 设置BOOT模式 */
    P_G_BOOTMODE = 0xE4,       /* 查询BOOT模式 */
    P_G_LD1_SW = 0xE5,         /* 查询LD1开关 */
    P_G_LD2_SW = 0xE6,         /* 查询LD2开关 */
    P_G_Q_SW = 0xE7,           /* 查询Q脉冲开关 */
    P_S_PulseType = 0xE8,      /* 设置脉冲类型  0x55：变频 0xAA：定频; */
    P_G_PulseType = 0xE9,      /* 查询脉冲类型 */
    P_Clear_Err = 0xEA,        /* 清空错误 */
    P_S_ALL_TEC_SW = 0x26,     /* 设置所有TEC开关  */
    P_S_ALL_LD_PARA = 0x27,    /* 设置所有LD参数 包括电流设定值和开关 */
    P_G_ALL_PARA = 0x28,       /* 查询所有参数 包括设置参数和检测参数 */
    P_S_LCM_MotorSpeed = 0x29, /* 设置LCM电机转速 */
    P_SAVE = 0x2A,             /* 参数保存 */
};

enum
{

    M_LD1_S_Cur = 0xA0,        /* 查询LD电流设定值通道1 */
    M_PULSE_WIDTH = 0xA3,      /* 查询脉宽设定值 */
    M_Q_DELAY = 0xA4,          /* 查询调Ｑ延时设定值 */
    M_TRG_TYPE = 0xA5,         /* 查询触发模式 0x55内触发；0xAA外触发*/
    M_LD1_SW = 0xA6,           /* LD1开关 */
    M_Q_SW = 0xA7,             /* todo 通信协议缺查询命令 查询Q开关 */
    M_TEC1_SW = 0xA8,          /* 查询TEC1开关 */
    M_TEC2_SW = 0xA9,          /* 查询TEC2开关 */
    M_TEC1_PARA = 0xAA,        /* 查询第1路TEC：温度、限压设定值 */
    M_TEC2_PARA = 0xAB,        /* 查询第2路TEC：温度、限压设定值 */
    M_PULSE_PARA = 0xAC,       /* 查询子脉冲个数及脉冲间隔设定值 */
    M_LD1_M_Cur = 0xAD,        /* 查询LD电流检测值通道1 */
    M_LD1_M_V = 0xAE,          /* 查询负载电压检测值通道1 */
    M_PWR_TEMP = 0xAF,         /* 查询电源温度检测值 */
    M_OUT_PD = 0xC0,           /* 查询外部输入电平1（PD）检测值 */
    M_OUT_TEMP = 0xC1,         /* 查询外部输入电平2（温度）检测值 */
    M_TEC1_M_TEMP = 0xC2,      /* 查询第1路检测温度检测值 */
    M_TEC1_M_PW = 0xC3,        /* 查询第1路TEC输出功率检测值 */
    M_TEC2_M_TEMP = 0xC4,      /* 查询第2路检测温度检测值 */
    M_TEC2_M_PW = 0xC5,        /* 查询第2路TEC输出功率检测值 */
    M_WRK_STA = 0xC6,          /* 查询工作状态 */
    M_ALL_SET = 0xC7,          /* 查询所有设定参数 */
    M_ALL_M = 0xCF,            /* 查询所有检测参数 */
    M_UPGRADE = 0xD0,          /* 在线程序升级 */
    M_LD2_SW = 0x20,           /* LD2开关 */
    M_LD2_S_Cur = 0xE0,        /* 查询LD电流设定值通道2 */
    M_TEC3_SW = 0x21,          /* 查询TEC3开关 */
    M_TEC4_SW = 0x22,          /* 查询TEC4开关 */
    M_TEC3_PARA = 0xEA,        /* 查询第3路TEC：温度、限压设定值 */
    M_TEC4_PARA = 0xEB,        /* 查询第4路TEC：温度、限压设定值 */
    M_LD2_M_Cur = 0xBD,        /* 查询LD电流检测值通道2 */
    M_LD2_M_V = 0xB9,          /* 查询负载电压检测值通道2 */
    M_TEC3_M_TEMP = 0xB2,      /* 查询第3路检测温度检测值 */
    M_TEC3_M_PW = 0xB3,        /* 查询第3路TEC输出功率检测值 */
    M_TEC4_M_TEMP = 0x23,      /* 查询第4路检测温度检测值 */
    M_TEC4_M_PW = 0x24,        /* 查询第4路TEC输出功率检测值 */
    M_BOOT_MODE = 0xEC,        /* BOOT模式 反馈 */
    M_PulseType = 0x25,        /* 查询脉冲类型 0x55：变频 0xAA：定频;  */
    M_ALL_PARA = 0xED,         /* 查询所有参数 包括设置参数和检测参数 */
    M_S_LCM_MotorSpeed = 0xEE, /* 查询LCM电机转速 */
    M_SAVE = 0xef,             /* 参数保存 反馈 */
};

void update_status()
{
    Work_Status2.PWR_OT = Work_Status.PWR_OT;
    Work_Status2.PWR_ERR = Work_Status.PWR_ERR;
    Work_Status2.EPPROM_ERR = Work_Status.EPPROM_ERR;
    Work_Status2.NTC1_ERR = Work_Status.NTC1_ERR | Work_Status.TEC1_OT | Work_Status.TEC1_UT;
    Work_Status2.NTC2_ERR = Work_Status.NTC2_ERR | Work_Status.TEC2_OT | Work_Status.TEC2_UT;
    Work_Status2.NTC3_ERR = Work_Status.NTC3_ERR | Work_Status.TEC3_OT | Work_Status.TEC3_UT;
    Work_Status2.NTC4_ERR = Work_Status.NTC4_ERR | Work_Status.TEC4_OT | Work_Status.TEC4_UT;
    Work_Status2.LD1_UC = Work_Status.LD1_UC;
    Work_Status2.LD1_SW = Work_Status.LD1_SW;
    Work_Status2.LD1_OC = Work_Status.LD1_OC;
    Work_Status2.LD1_SW = Work_Status.LD1_SW;
    Work_Status2.LD2_UC = Work_Status.LD2_UC;
    Work_Status2.LD2_SW = Work_Status.LD2_SW;
    Work_Status2.LD2_OC = Work_Status.LD2_OC;
    Work_Status2.TEC1_SW = Work_Status.TEC1_SW;
    Work_Status2.TEC1_SW = Work_Status.TEC1_SW;
    Work_Status2.TEC2_SW = Work_Status.TEC2_SW;
    Work_Status2.TEC3_SW = Work_Status.TEC3_SW;
    Work_Status2.TEC4_SW = Work_Status.TEC4_SW;
    Work_Status2.Q_SW = Work_Status.Q_SW;
    Work_Status2.TRQ_Type = Work_Status.TRQ_Type;
    Work_Status2.Pulse_Type = Work_Status.Pulse_Type;
    Work_Status2.LCM_Fan = set_param.LCM.Fan == WORK_ON ? 1 : 0;
    Work_Status2.LCM_MotorEn = set_param.LCM.MotorEn == WORK_ON ? 1 : 0;
    // sizeof(Work_Status2);
}

static void uart_send(UART_HandleTypeDef *huart, uint8_t cmd, void *data, uint8_t dataLen)
{
    static uint8_t UartTxBuff[MAX_SIZE];//共用一个发送缓存 极端情况下可能会资源冲突
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
    HAL_UART_Transmit_DMA(huart, UartTxBuff, length);
}

static void send_ack(UART_HandleTypeDef *huart, uint8_t cmd, void *data, uint8_t dataLen)
{
    if (CheckUartReady(huart))
    {
        uart_send(huart, cmd, data, dataLen);
    }
}


static void set_tec_param(uint8_t ch, uint8_t *data)
{
    tec_packet_t *p = data;
    tec_setparam_t *tec = &set_param.tec[ch];
    if (p->Temp >= TEC_SET_MIN_TEMP && p->Temp <= TEC_SET_MAX_TEMP)
    {
        tec->Temp = p->Temp;
    }
    tec->MaxVol = p->V;
    if (tec->sw == WORK_ON)
    {
        TEC_RestStatus(ch);
    }
}

static void set_all_tec_sw(uint8_t *data)
{
    for (size_t i = 0; i < 4; i++)
    {
        uint8_t sw = data[i];
        if (sw == 0x55 || sw == 0xAA)
        {
            if (sw == WORK_ON && set_param.tec[i].sw == WORK_OFF)
            {
                TEC_RestStatus(i);
            }
            set_param.tec[i].sw = sw;
        }
    }
}

static void set_all_ld_para(uint8_t *data)
{
    typedef struct
    {
        uint8_t sw;   // 开关 0x55:关；0xAA：开
        uint16_t Cur; // 电流设定值 单位 0.1A
    } all_ld_packet_t;
    all_ld_packet_t *p = (all_ld_packet_t *)data;
    for (size_t i = 0; i < 2; i++)
    {
        uint8_t sw = p[i].sw;
        uint16_t Cur = p[i].Cur;
        if (sw == 0x55 || sw == 0xAA)
        {
            set_param.ld[i].sw = sw;
        }
        set_param.ld[i].Cur = Cur;
    }
}

static void set_LCM_MotorSpeed(UART_HandleTypeDef *huart, uint8_t *data)
{
    uint16_t speed = *(uint16_t *)data;
    set_param.LCM.MotorSpeed = speed;
    pump_setting(&pump_set_param);
    // PC_ACK(M_S_LCM_MotorSpeed, speed);
}

static void get_tec_param(UART_HandleTypeDef *huart, uint8_t cmd, uint8_t ch)
{
    tec_packet_t p;
    tec_setparam_t *tec = &set_param.tec[ch];
    p.Temp = tec->Temp;
    p.V = tec->MaxVol;
    PC_ACK(cmd, p);
}

static void ack_pulse_param(UART_HandleTypeDef *huart, uint8_t cmd)
{
    Pulse_param_packet_t p = get_pulse_param();
    PC_ACK(cmd, p);
}

static void get_all_set_param(UART_HandleTypeDef *huart, uint8_t cmd)
{

    set_param_t *data = &set_param;
    set_packet_t p;
    p.LD1_Cur = data->ld[0].Cur;
    p.LD2_Cur = data->ld[1].Cur;
    p.PulseWidth = data->Pulse_para.Width;
    p.Q_DELAY = data->T_Q.delay;
    p.TEC1_Temp = data->tec[0].Temp;
    p.TEC1_V = data->tec[0].MaxVol;
    p.TEC2_Temp = data->tec[1].Temp;
    p.TEC2_V = data->tec[1].MaxVol;
    p.TEC3_Temp = data->tec[2].Temp;
    p.TEC3_V = data->tec[2].MaxVol;
    p.TEC4_Temp = data->tec[3].Temp;
    p.TEC4_V = data->tec[3].MaxVol;
    p.Pulse_para.Num = data->Pulse_para.Num;
    for (size_t i = 0; i < p.Pulse_para.Num; i++)
    {
        p.Pulse_para.Interval[i] = data->Pulse_para.Interval[i];
    }
    //  sizeof(p);
    PC_ACK(cmd, p);
}

static void get_all_measure_param(UART_HandleTypeDef *huart, uint8_t cmd)
{
    measure_param_t *data = &measure_param;
    measure_packet_t p;
    p.LD1_Cur = round(data->ld[0].Cur * 10);
    p.LD2_Cur = round(data->ld[1].Cur * 10);
    p.LD1_V = round(data->ld[0].Vol * 10);
    p.LD2_V = round(data->ld[1].Vol * 10);
    p.PWR_Temp = round(data->PWR_Temp * 10);
    p.OUT_PD = round(data->out.PD * 10);
    p.OUT_TEMP = round(data->out.Temp * 10);
    p.TEC1_Temp = round(data->tec[0].Temp * 10);
    p.TEC1_Power = round(data->tec[0].Power * 10);
    p.TEC2_Temp = round(data->tec[1].Temp * 10);
    p.TEC2_Power = round(data->tec[1].Power * 10);
    p.TEC3_Temp = round(data->tec[2].Temp * 10);
    p.TEC3_Power = round(data->tec[2].Power * 10);
    p.TEC4_Temp = round(data->tec[3].Temp * 10);
    p.TEC4_Power = round(data->tec[3].Power * 10);
    p.LCM_Temp = round(data->LCM.Temp * 10);
    update_status();
    p.Work_Status = Work_Status2;
    for (size_t i = 0; i < 4; i++)
    {
        p.Version[i] = Version2[i];
    }
    p.LCM_Temp = data->LCM.Temp;
    p.LCM_Set_MotorSpeed = set_param.LCM.MotorSpeed;
    p.LCM_M_MotorSpeed = measure_param.LCM.MotorSpeed;
    p.LCM_M_state = measure_param.LCM.state;
    // sizeof(p);
    PC_ACK(cmd, p);
}

static void get_all_para(UART_HandleTypeDef *huart, uint8_t cmd)
{
    typedef struct
    {
        set_packet_t set;
        measure_packet_t measure;
    } all_para_packet_t;
    all_para_packet_t p;
    set_param_t *set = &set_param;
    p.set.LD1_Cur = set->ld[0].Cur;
    p.set.LD2_Cur = set->ld[1].Cur;
    p.set.PulseWidth = set->Pulse_para.Width;
    p.set.Q_DELAY = set->T_Q.delay;
    p.set.TEC1_Temp = set->tec[0].Temp;
    p.set.TEC1_V = set->tec[0].MaxVol;
    p.set.TEC2_Temp = set->tec[1].Temp;
    p.set.TEC2_V = set->tec[1].MaxVol;
    p.set.TEC3_Temp = set->tec[2].Temp;
    p.set.TEC3_V = set->tec[2].MaxVol;
    p.set.TEC4_Temp = set->tec[3].Temp;
    p.set.TEC4_V = set->tec[3].MaxVol;
    p.set.Pulse_para.Num = set->Pulse_para.Num;
    for (size_t i = 0; i < p.set.Pulse_para.Num; i++)
    {
        p.set.Pulse_para.Interval[i] = set->Pulse_para.Interval[i];
    }
    measure_param_t *data = &measure_param;
    p.measure.LD1_Cur = round(data->ld[0].Cur * 10);
    p.measure.LD2_Cur = round(data->ld[1].Cur * 10);
    p.measure.LD1_V = round(data->ld[0].Vol * 10);
    p.measure.LD2_V = round(data->ld[1].Vol * 10);
    p.measure.PWR_Temp = round(data->PWR_Temp * 10);
    p.measure.OUT_PD = round(data->out.PD * 10);
    p.measure.OUT_TEMP = round(data->out.Temp * 10);
    p.measure.TEC1_Temp = round(data->tec[0].Temp * 10);
    p.measure.TEC1_Power = round(data->tec[0].Power * 10);
    p.measure.TEC2_Temp = round(data->tec[1].Temp * 10);
    p.measure.TEC2_Power = round(data->tec[1].Power * 10);
    p.measure.TEC3_Temp = round(data->tec[2].Temp * 10);
    p.measure.TEC3_Power = round(data->tec[2].Power * 10);
    p.measure.TEC4_Temp = round(data->tec[3].Temp * 10);
    p.measure.TEC4_Power = round(data->tec[3].Power * 10);
    p.measure.LCM_Temp = round(data->LCM.Temp * 10);
    update_status();
    p.measure.Work_Status = Work_Status2;
    for (size_t i = 0; i < 4; i++)
    {
        p.measure.Version[i] = Version2[i];
    }
    p.measure.LCM_Temp = data->LCM.Temp;
    p.measure.LCM_Set_MotorSpeed = set_param.LCM.MotorSpeed;
    p.measure.LCM_M_MotorSpeed = measure_param.LCM.MotorSpeed;
    p.measure.LCM_M_state = measure_param.LCM.state;
    // sizeof(p);
    PC_ACK(cmd, p);
}

void get_work_status(UART_HandleTypeDef *huart, uint8_t cmd)
{
    update_status();
    PC_ACK(cmd, Work_Status2);
}

static void ack_boot_mode(UART_HandleTypeDef *huart, uint8_t cmd)
{
    uint8_t mode = get_bootmode();
    PC_ACK(cmd, mode);
}

void exec_commands_list2(UART_HandleTypeDef *huart, uint8_t cmd, uint8_t *data, size_t data_len)
{
     
   // *(float *)data
   switch (cmd)
   {
    
   case P_S_LD1_S_Cur:              SET_PARAM_INT16(set_param.ld[0].Cur); PC_ACK_BK;                            break;
   case P_S_LD2_S_Cur:              SET_PARAM_INT16(set_param.ld[1].Cur); PC_ACK_BK;                            break;
   case P_S_PULSE_WIDTH:            set_pulse_width(*(uint16_t *)data); PC_ACK_BK;                              break;
   case P_S_Q_DELAY:                set_Q_delay(*(uint16_t *)data); PC_ACK_BK;                                  break;

   case P_S_TRG_TYPE:               set_TRG_type(*(uint8_t *)data); PC_ACK_BK;                                  break;
   case P_S_PulseType:              set_pulse_type(*(uint8_t *)data); PC_ACK_BK;                                break;
   case P_S_LD1_SW:                 SET_SW_STA(set_param.ld[0].sw); PC_ACK_BK;                                  break;
   case P_S_LD2_SW:                 SET_SW_STA(set_param.ld[1].sw); PC_ACK_BK;                                  break;
   case P_S_Q_SW:                   SET_SW_STA(set_param.T_Q.sw); PC_ACK_BK;                                    break;
   case P_S_TEC1_SW:                SET_SW_STA(set_param.tec[0].sw); PC_ACK_BK;                                 break;
   case P_S_TEC2_SW:                SET_SW_STA(set_param.tec[1].sw); PC_ACK_BK;                                 break;
   case P_S_TEC3_SW:                SET_SW_STA(set_param.tec[2].sw); PC_ACK_BK;                                 break;
   case P_S_TEC4_SW:                SET_SW_STA(set_param.tec[3].sw); PC_ACK_BK;                                 break;
   case P_S_TEC1_PARA:              set_tec_param(0, data); PC_ACK_BK;                                          break;
   case P_S_TEC2_PARA:              set_tec_param(1, data); PC_ACK_BK;                                          break;
   case P_S_TEC3_PARA:              set_tec_param(2, data); PC_ACK_BK;                                          break;
   case P_S_TEC4_PARA:              set_tec_param(3, data); PC_ACK_BK;                                          break;
   case P_S_PULSE_PARA:             set_pulse_param(data); PC_ACK_BK;                                           break;
   case P_S_LCM_MotorSpeed:         set_LCM_MotorSpeed(huart, data); PC_ACK_BK;                                 break;

   case P_G_PULSE_WIDTH:            PC_ACK(M_PULSE_WIDTH,set_param.Pulse_para.Width);                           break;
   case P_G_Q_DELAY:                PC_ACK(M_Q_DELAY, set_param.T_Q.delay);                                     break;
   case P_G_TRG_TYPE:               PC_ACK(M_TRG_TYPE, set_param.TRG_Type);                                     break;
   case P_G_LD1_S_Cur:              PC_ACK(M_LD1_S_Cur, set_param.ld[0].Cur);                                   break;
   case P_G_LD2_S_Cur:              PC_ACK(M_LD2_S_Cur, set_param.ld[1].Cur);                                   break;
   case P_G_TEC1_SW:                PC_ACK(M_TEC1_SW, set_param.tec[0].sw);                                     break;
   case P_G_TEC2_SW:                PC_ACK(M_TEC2_SW, set_param.tec[1].sw);                                     break;
   case P_G_TEC3_SW:                PC_ACK(M_TEC3_SW, set_param.tec[2].sw);                                     break;
   case P_G_TEC4_SW:                PC_ACK(M_TEC4_SW, set_param.tec[3].sw);                                     break;
   case P_G_TEC1_PARA:              get_tec_param(huart, M_TEC1_PARA, 0);                                       break;
   case P_G_TEC2_PARA:              get_tec_param(huart, M_TEC2_PARA, 1);                                       break;
   case P_G_TEC3_PARA:              get_tec_param(huart, M_TEC3_PARA, 2);                                       break;
   case P_G_TEC4_PARA:              get_tec_param(huart, M_TEC4_PARA, 3);                                       break;
   case P_G_PULSE_PARA:             ack_pulse_param(huart, M_PULSE_PARA);                                       break;
   case P_G_LD1_M_Cur:              PC_ACK_UINT16(M_LD1_M_Cur, measure_param.ld[0].Cur * 10);                   break;
   case P_G_LD2_M_Cur:              PC_ACK_UINT16(M_LD2_M_Cur, measure_param.ld[1].Cur * 10);                   break;
   case P_G_LD1_M_V:                PC_ACK_UINT16(M_LD1_M_V, measure_param.ld[0].Vol * 10);                     break;
   case P_G_LD2_M_V:                PC_ACK_UINT16(M_LD2_M_V, measure_param.ld[1].Vol * 10);                     break;
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
    
   case P_G_Q_SW:                   PC_ACK(M_Q_SW, set_param.T_Q.sw);                                           break;
   case P_G_WRK_STA:                get_work_status(huart, M_WRK_STA);                                          break;
   case P_G_ALL_SET:                get_all_set_param(huart, M_ALL_SET);                                        break;
   case P_G_ALL_M:                  get_all_measure_param(huart, M_ALL_M);                                      break; 

   case P_G_PulseType:              PC_ACK(M_PulseType, set_param.Pulse_Type);                                  break;
   case P_G_LD1_SW:                 PC_ACK(M_LD1_SW, set_param.ld[0].sw);                                       break;
   case P_G_LD2_SW:                 PC_ACK(M_LD2_SW, set_param.ld[1].sw);                                       break;

   case P_S_BOOTMODE:               set_boot_bootmode(*(uint8_t *)data);ack_boot_mode(huart, M_BOOT_MODE);      break;
   case P_G_BOOTMODE:               ack_boot_mode(huart, M_BOOT_MODE);                                          break;
   case P_Clear_Err:                ClearErrs();                                                                break;
   case P_S_ALL_TEC_SW:             set_all_tec_sw(data); PC_ACK_BK;                                            break;
   case P_S_ALL_LD_PARA:            set_all_ld_para(data); PC_ACK_BK;                                           break;
   case P_G_ALL_PARA:               get_all_para(huart, M_ALL_PARA);                                            break;
   case P_SAVE:                     save_param(huart,M_SAVE);                                                   break;
   default:
       break;
   }
   
}





