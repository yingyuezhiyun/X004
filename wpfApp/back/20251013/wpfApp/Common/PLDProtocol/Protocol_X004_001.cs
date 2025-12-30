using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wpfApp.Common.CtrlProtocol;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace wpfApp.Common.PLDProtocol
{

   
 

    public class PLDParams
    {
        
        
        public PulseParams pulseParams = new PulseParams();

        public class PulseParams
        {
            public UInt16 Num;
            public UInt16[] Interval = new UInt16[11];
        }

        public TECParams tECParams = new TECParams();
        public class TECParams
        {
            public float Temp;
            public float Vol;
            public UInt16 Mode;
        }

        public CalibCoefParams CalibCoef = new CalibCoefParams();
        public class CalibCoefParams
        {
            public float k;
            public float b;
        }

        public PIDCoefParams PIDCoef= new PIDCoefParams();
        public class PIDCoefParams
        {
            public float p;
            public float i;
            public float d;
        }

        public LCMSetParams LCMSet = new LCMSetParams();
        public class LCMSetParams
        {
            public byte PwrEn;        // 电泵供电开关
            public byte MotorEn;      // 电机开关
            public UInt16 MotorSpeed; // 转速 1rpm
            public byte Fan;          // 风扇 100代表100占空比
            public byte PwrLimit;     // 功率限制 W

        }

        public LCMMeasureParams LCMMeasure = new LCMMeasureParams();
        public class LCMMeasureParams
        {

            public byte MotorEn;      // 电机开关
            public UInt16 MotorSpeed; // 转速 1rpm
            public float DCVoltage;   // 直流母线电压 0.1V
            public float DCCurrent;   // 直流母线电流 0.1A
            public float Temp;        // 电机控制器温度 0.1℃       
            public byte PwrLimit;     // 功率 W
        }

        [Flags]
        public enum StatusFlags : UInt32
        {
            PWR_OT = 1 << 0,     // 电源过热
            PWR_ERR = 1 << 1,    // 电路故障
            LD1_UV = 1 << 2,     // LD1驱动欠压
            LD1_OV = 1 << 3,     // LD1驱动过压
            LD2_UV = 1 << 4,     // LD2驱动欠压
            LD2_OV = 1 << 5,     // LD2驱动过压
            EPPROM_ERR = 1 << 6, // 参数存储错误
            NTC1_ERR = 1 << 7,   // 热敏电阻1异常
            NTC2_ERR = 1 << 8,   // 热敏电阻2异常
            NTC3_ERR = 1 << 9,   // 热敏电阻3异常
            NTC4_ERR = 1 << 10,   // 热敏电阻4异常
            TEC1_SW = 1 << 11,    // TEC1 开关 1：开启; 0：关闭
            TEC2_SW = 1 << 12,    // TEC2 开关 1：开启; 0：关闭
            TEC3_SW = 1 << 13,    // TEC3 开关 1：开启; 0：关闭
            TEC4_SW = 1 << 14,    // TEC4 开关 1：开启; 0：关闭
            Q_SW = 1 << 15,       // 调Q 开关 1：开启; 0：关闭
            LD1_SW = 1 << 16,     // LD1 开关 1：开启; 0：关闭
            LD2_SW = 1 << 17,     // LD2 开关 1：开启; 0：关闭
            TRQ_STA = 1 << 18,    // 触发状态 1：外触发; 0：内触发
        }


        public class Status
        {
            public StatusFlags statusFlags;
            public string Name;
        }
        Status[] ErrStatus =
        {
            new Status() { Name= "电源过热 ",statusFlags=StatusFlags.PWR_OT },
            new Status() { Name= "电路故障 ",statusFlags=StatusFlags.PWR_ERR },
            new Status() { Name= "LD1驱动欠压 ",statusFlags=StatusFlags.LD1_UV },
            new Status() { Name= "LD1驱动过压 ",statusFlags=StatusFlags.LD1_OV },
            new Status() { Name= "LD2驱动欠压 ",statusFlags=StatusFlags.LD2_UV },
            new Status() { Name= "LD2驱动过压 ",statusFlags=StatusFlags.LD2_OV },
            new Status() { Name= "参数存储错误 ",statusFlags=StatusFlags.EPPROM_ERR },
            new Status() { Name= "热敏电阻1异常 ",statusFlags=StatusFlags.NTC1_ERR },
            new Status() { Name= "热敏电阻2异常 ",statusFlags=StatusFlags.NTC2_ERR },
            new Status() { Name= "热敏电阻3异常 ",statusFlags=StatusFlags.NTC3_ERR },
            new Status() { Name= "热敏电阻4异常 ",statusFlags=StatusFlags.NTC4_ERR },
        };

        Status[] RunningStatus =
        {
            new Status() { Name= "TEC1开启 ",statusFlags=StatusFlags.TEC1_SW },
            new Status() { Name= "TEC2开启 ",statusFlags=StatusFlags.TEC1_SW },
            new Status() { Name= "TEC3开启 ",statusFlags=StatusFlags.TEC1_SW },
            new Status() { Name= "TEC4开启 ",statusFlags=StatusFlags.TEC1_SW },
            new Status() { Name= "调Q开启",statusFlags=StatusFlags.Q_SW },
            new Status() { Name= "LD1开启",statusFlags=StatusFlags.LD1_SW },
            new Status() { Name= "LD2开启",statusFlags=StatusFlags.LD2_SW },
            new Status() { Name= "外触发 ",statusFlags=StatusFlags.TRQ_STA },           
        };


        public enum PLDParamsFromGet
        {

            None,
            LD1_S_Cur = 0xA0,   /* 查询LD电流设定值通道1 */
            LD2_S_Cur = 0xF0,   /* 查询LD电流设定值通道2 */
            DFLT_V = 0xA1,      /* 查询初始电压设定值 */
            INTER_TRG_FREQ,     /* 查询内触发频率设定值 */
            PULSE_WIDTH,        /* 查询脉宽设定值 */
            Q_DELAY,            /* 查询调Ｑ延时设定值 */
            TRG_TYPE,           /* 查询触发模式 */
            LD1_SW,             /* LD1开关 */
            LD2_SW = 0xF6,      /* LD2开关 */
            Q_SW = 0xA7,        /* todo 查询Q脉冲设定值 */
            TEC1_SW,            /* 查询TEC1开关 */
            TEC2_SW,            /* 查询TEC2开关 */
            TEC3_SW = 0xF8,     /* 查询TEC3开关 */
            TEC4_SW,            /* 查询TEC4开关 */
            TEC1_PARA = 0xAA,   /* 查询第1路TEC：温度、限压、模式设定值 */
            TEC2_PARA,          /* 查询第2路TEC：温度、限压、模式设定值 */
            TEC3_PARA = 0xFA,   /* 查询第3路TEC：温度、限压、模式设定值 */
            TEC4_PARA,          /* 查询第4路TEC：温度、限压、模式设定值 */
            PULSE_PARA = 0xAC,  /* 查询子脉冲个数及脉冲间隔设定值 */
            LD1_M_Cur,          /* 查询LD电流检测值通道1 */
            LD2_M_Cur = 0xFD,   /* 查询LD电流检测值通道2 */
            L1_M_V = 0xAE,      /* 查询负载电压检测值通道1 */
            L2_M_V = 0xEE,      /* 查询负载电压检测值通道2 */
            PWR_TEMP = 0xAF,    /* 查询电源温度检测值 */
            OUT_PD = 0xC0,      /* 查询外部输入电平1（PD）检测值 */
            OUT_TEMP,           /* 查询外部输入电平2（温度）检测值 */
            TEC1_M_TEMP,        /* 查询第1路检测温度检测值 */
            TEC1_M_PW,          /* 查询第1路TEC输出功率检测值 */
            TEC2_M_TEMP,        /* 查询第2路检测温度检测值 */
            TEC2_M_PW,          /* 查询第2路TEC输出功率检测值 */
            TEC3_M_TEMP = 0x12, /* 查询第3路检测温度检测值 */
            TEC3_M_PW,          /* 查询第3路TEC输出功率检测值 */
            TEC4_M_TEMP,        /* 查询第4路检测温度检测值 */
            TEC4_M_PW,          /* 查询第4路TEC输出功率检测值 */
            WRK_STA = 0xC6,     /* 查询工作状态 */
            ALL_SET,            /* 查询所有设定参数 */
            ALL_M = 0xCF,       /* 查询所有检测参数 */
            UPGRADE = 0xD0,     /* 在线程序升级 */

            ////////////////////////////////////////////////
            LD1_Vol = 0x20, /* 查询LD1电压值 206～338代表20.6V～33.8V */
            LD2_Vol,        /* 查询LD2电压值 206～338代表20.6V～33.8V */
            LD1_HOC,        /* 查询LD1硬件过流  10～150代表1.0A～15.0A*/
            LD2_HOC,        /* 查询LD1硬件过流  10～150代表1.0A～15.0A*/
            LD1_SKB,        /* 查询LD1电流设定值标定参数 */
            LD2_SKB,        /* 查询LD2电流设定值标定参数 */
            LD1_MKB,        /* 查询LD1电流测量值标定参数 */
            LD2_MKB,        /* 查询LD2电流测量值标定参数 */
            PD_MKB,         /* 查询外部输入电平1（PD）（光功率检测值）检测值标定参数 */
            TEC1_PID,       /* 查询TEC1的PID参数 */
            TEC2_PID,       /* 查询TEC2的PID参数 */
            TEC3_PID,       /* 查询TEC3的PID参数 */
            TEC4_PID,       /* 查询TEC4的PID参数 */
            S_LCM,          /* 查询液冷模块设置参数 */
            M_LCM,          /* 查询液冷模块反馈参数 */


        }
      

        public enum PLDParamsToSet
        {
            LD1_S_Cur = 0x30,  /* 电流通道1 10～150代表1.0A～15.0A */
            LD2_S_Cur = 0x90,  /* 电流通道2 10～150代表1.0A～15.0A */
            DFLT_V = 0x31,     /* 初始电压 20～45，代表20V～45V */
            INTER_TRG_FREQ,    /* 内触发频率 1Hz～1000Hz */
            PULSE_WIDTH,       /* 脉宽 100us～250us */
            Q_DELAY,           /* 调Ｑ延时 50us～300us */
            TRG_TYPE,          /* 触发模式 0x55内触发；0xAA外触发 */
            LD1_SW,            /* LD电流开关通道1 0x55:关LD电流；0xAA：开LD电流。上电默认关 */
            LD2_SW = 0x96,     /* LD电流开关通道2 0x55:关LD电流；0xAA：开LD电流。上电默认关 */
            Q_SW = 0x37,       /* Q脉冲开关 0x55:关Ｑ脉冲；0xAA：开Ｑ脉冲。上电默认开 */
            TEC1_SW,           /* TEC1开关 0x55：关TEC1；0xAA：开TEC1。上电默认开 */
            TEC2_SW,           /* TEC2开关 0x55：关TEC2；0xAA：开TEC2。上电默认开 */
            TEC3_SW = 0x98,    /* TEC3开关 0x55：关TEC3；0xAA：开TEC3。上电默认开 */
            TEC4_SW,           /* TEC4开关 0x55：关TEC4；0xAA：开TEC4。上电默认开 */
            TEC1_PARA = 0x3A,  /* 第1路TEC：工作温度、限压、模式 设置TEC1参数，共3个参数，每个参数2个字节：1、工作温度（150～650，代表15.0℃～65.0℃）2、限压3、模式 */
            TEC2_PARA,         /* 第2路TEC */
            TEC3_PARA = 0x9A,  /* 第3路TEC */
            TEC4_PARA,         /* 第4路TEC */
            PULSE_PARA = 0x3C, /* 子脉冲参数，共12个参数，每个参数2个字节：1、子脉冲个数（取值范围1~12）2~11、脉冲间隔1（取值范围240~660）*/


            P_UPGRADE = 0x76,          /* 在线程序升级 */

            ////////////////////////////////////////////////


            LD1_Vol = 0xC0, /* LD1电压值 206～338代表20.6V～33.8V */
            LD2_Vol,        /* LD2电压值 206～338代表20.6V～33.8V */
            LD1_HOC,        /* LD1硬件过流  10～150代表1.0A～15.0A*/
            LD2_HOC,        /* LD1硬件过流  10～150代表1.0A～15.0A*/
            LD1_SKB,        /* LD1电流设定值标定参数 */
            LD2_SKB,        /* LD2电流设定值标定参数 */
            LD1_MKB,        /* LD1电流测量值标定参数 */
            LD2_MKB,        /* LD2电流测量值标定参数 */
            PD_MKB,         /* 外部输入电平1（PD）（光功率检测值）检测值标定参数 */
            TEC1_PID,       /* TEC1的PID参数 */
            TEC2_PID,       /* TEC2的PID参数 */
            TEC3_PID,       /* TEC3的PID参数 */
            TEC4_PID,       /* TEC4的PID参数 */
            LCM,            /* 液冷模块参数 */
        }

        public enum PLDParamsToQuery
        {
            Cur = 0x3D,         /* 电流设定值 */
            DFLT_V,             /* 初始电压设定值 */
            INTER_TRG_FREQ,     /* 内触发频率设定值 */
            PULSE_WIDTH = 0x50, /* 脉宽设定值 */
            Q_DELAY,            /* 调Ｑ延时设定值 */
            TRG_TYPE,           /* 触发模式 */
            LD1_S_Cur,          /* LD电流设定值通道1 */
            LD2_S_Cur = 0xB3,   /* LD电流设定值通道2 */
            Q_SW = 0x54,        /* Q开关 */
            TEC1_SW,            /* TEC1开关 */
            TEC2_SW,            /* TEC2开关 */
            TEC3_SW = 0xB5,     /* TEC3开关 */
            TEC4_SW,            /* TEC4开关 */
            TEC1_PARA = 0x57,   /* 第1路TEC：温度、限压、模式设定值 */
            TEC2_PARA,          /* 第2路TEC：温度、限压、模式设定值 */
            TEC3_PARA = 0xB7,   /* 第3路TEC：温度、限压、模式设定值 */
            TEC4_PARA,          /* 第4路TEC：温度、限压、模式设定值 */
            PULSE_PARA = 0x59,  /* 子脉冲个数及脉冲间隔设定值 */
            LD1_M_Cur,          /* LD电流检测值通道1 */
            LD2_M_Cur = 0xBA,   /* LD电流检测值通道2 */
            L1_M_V = 0x5B,      /* 负载电压检测值通道1 */
            L2_M_V = 0xBB,      /* 负载电压检测值通道2 */
            PWR_TEMP = 0x5C,    /* 电源温度检测值 */
            OUT_PD,             /* 外部输入电平1（PD）检测值 */
            OUT_TEMP,           /* 外部输入电平2（温度）检测值 */
            TEC1_M_TEMP,        /* 第1路检测温度检测值 */
            TEC1_M_PW = 0x70,   /* 第1路TEC输出功率检测值 */
            TEC2_M_TEMP,        /* 第2路检测温度检测值 */
            TEC2_M_PW,          /* 第2路TEC输出功率检测值 */
            TEC3_M_TEMP = 0xBF, /* 第3路检测温度检测值 */
            TEC3_M_PW = 0xE0,   /* 第3路TEC输出功率检测值 */
            TEC4_M_TEMP,        /* 第4路检测温度检测值 */
            TEC4_M_PW,          /* 第4路TEC输出功率检测值 */

            WRK_STA = 0x73, /* 工作状态 */
            ALL_SET,        /* 所有设定参数 */
            ALL_M,          /* 所有检测参数 */

            LD1_SW = 0x77, /* LD1开关 */
            LD2_SW = 0xE7, /* LD2开关 */

            LD1_Vol = 0xD0, /* LD1电压值 206～338代表20.6V～33.8V */
            LD2_Vol,        /* LD2电压值 206～338代表20.6V～33.8V */
            LD1_HOC,        /* LD1硬件过流  10～150代表1.0A～15.0A*/
            LD2_HOC,        /* LD1硬件过流  10～150代表1.0A～15.0A*/
            LD1_SKB,        /* LD1电流设定值标定参数 */
            LD2_SKB,        /* LD2电流设定值标定参数 */
            LD1_MKB,        /* LD1电流测量值标定参数 */
            LD2_MKB,        /* LD2电流测量值标定参数 */
            PD_MKB,         /* 外部输入电平1（PD）（光功率检测值）检测值标定参数 */
            TEC1_PID,       /* TEC1的PID参数 */
            TEC2_PID,       /* TEC2的PID参数 */
            TEC3_PID,       /* TEC3的PID参数 */
            TEC4_PID,       /* TEC4的PID参数 */
            S_LCM,          /* 液冷模块设置参数 */
            M_LCM,          /* 液冷模块反馈参数 */
        }
        public enum TRIGTYPE
        {
            INTER = 0x55,
            OUT = 0xAA
        }
       

    }

  
    public class Protocol_X004_001 : ICommunicationProtocol2
    {


        public Protocol_X004_001()
        {
            Thread thread = new Thread(new ThreadStart(() => { this.dataHandler(); }));
        }

        

        private class PLDRev
        {
            public List<byte> rawData { get; set; } = new List<byte>();

            public byte cmd { get { return this.rawData[4]; } }

            public int dataLen { get { return this.rawData[5]; } }

            public byte[] data { get { return rawData.Skip(6).Take(this.dataLen).ToArray(); } }

            public float ToUFloat { get { return (BitConverter.ToUInt16(this.data, 0)) / 10.0f; } }

            public float ToSFloat { get { return (BitConverter.ToInt16(this.data, 0)) / 10.0f; } }

            public UInt16 ToUInt16 { get { return BitConverter.ToUInt16(this.data, 0); } }


            public DateTime time { get; set; }
        }

        private List<PLDRev> revdatas = new List<PLDRev>();

        private List<byte> remainData = new List<byte>();

        private object locker = new object();

        private const UInt16 CMD_HEAD = 0x5A54;
        private const UInt16 CMD_TAIL = 0x5AFE;
        private const byte PC_ID = 0x14;
        private const byte MCU_ID = 0x1c;

        private const byte WorkON = 0xaa;
        private const byte WorkOFF = 0x55;


        private enum DEV_QUERY_CMD_TYPE
        {
            Cur = 0x3D,         /* 电流设定值 */
            DFLT_V,             /* 初始电压设定值 */
            INTER_TRG_FREQ,     /* 内触发频率设定值 */
            PULSE_WIDTH = 0x50, /* 脉宽设定值 */
            Q_DELAY,            /* 调Ｑ延时设定值 */
            TRG_TYPE,           /* 触发模式 */
            LD1_S_Cur,          /* LD电流设定值通道1 */
            LD2_S_Cur = 0xB3,   /* LD电流设定值通道2 */
            Q_SW = 0x54,        /* Q开关 */
            TEC1_SW,            /* TEC1开关 */
            TEC2_SW,            /* TEC2开关 */
            TEC3_SW = 0xB5,     /* TEC3开关 */
            TEC4_SW,            /* TEC4开关 */
            TEC1_PARA = 0x57,   /* 第1路TEC：温度、限压、模式设定值 */
            TEC2_PARA,          /* 第2路TEC：温度、限压、模式设定值 */
            TEC3_PARA = 0xB7,   /* 第3路TEC：温度、限压、模式设定值 */
            TEC4_PARA,          /* 第4路TEC：温度、限压、模式设定值 */
            PULSE_PARA = 0x59,  /* 子脉冲个数及脉冲间隔设定值 */
            LD1_M_Cur,          /* LD电流检测值通道1 */
            LD2_M_Cur = 0xBA,   /* LD电流检测值通道2 */
            L1_M_V = 0x5B,      /* 负载电压检测值通道1 */
            L2_M_V = 0xBB,      /* 负载电压检测值通道2 */
            PWR_TEMP = 0x5C,    /* 电源温度检测值 */
            OUT_PD,             /* 外部输入电平1（PD）检测值 */
            OUT_TEMP,           /* 外部输入电平2（温度）检测值 */
            TEC1_M_TEMP,        /* 第1路检测温度检测值 */
            TEC1_M_PW = 0x70,   /* 第1路TEC输出功率检测值 */
            TEC2_M_TEMP,        /* 第2路检测温度检测值 */
            TEC2_M_PW,          /* 第2路TEC输出功率检测值 */
            TEC3_M_TEMP = 0xBF, /* 第3路检测温度检测值 */
            TEC3_M_PW = 0xE0,   /* 第3路TEC输出功率检测值 */
            TEC4_M_TEMP,        /* 第4路检测温度检测值 */
            TEC4_M_PW,          /* 第4路TEC输出功率检测值 */

            WRK_STA = 0x73, /* 工作状态 */
            ALL_SET,        /* 所有设定参数 */
            ALL_M,          /* 所有检测参数 */

            LD1_SW = 0x77, /* LD1开关 */
            LD2_SW = 0xE7, /* LD2开关 */

            LD1_Vol = 0xD0, /* LD1电压值 206～338代表20.6V～33.8V */
            LD2_Vol,        /* LD2电压值 206～338代表20.6V～33.8V */
            LD1_HOC,        /* LD1硬件过流  10～150代表1.0A～15.0A*/
            LD2_HOC,        /* LD1硬件过流  10～150代表1.0A～15.0A*/
            LD1_SKB,        /* LD1电流设定值标定参数 */
            LD2_SKB,        /* LD2电流设定值标定参数 */
            LD1_MKB,        /* LD1电流测量值标定参数 */
            LD2_MKB,        /* LD2电流测量值标定参数 */
            PD_MKB,         /* 外部输入电平1（PD）（光功率检测值）检测值标定参数 */
            TEC1_PID,       /* TEC1的PID参数 */
            TEC2_PID,       /* TEC2的PID参数 */
            TEC3_PID,       /* TEC3的PID参数 */
            TEC4_PID,       /* TEC4的PID参数 */
            S_LCM,          /* 液冷模块设置参数 */
            M_LCM,          /* 液冷模块反馈参数 */
        }
        private enum DEV_SET_CMD_TYPE
        {
            LD1_S_Cur = 0x30,  /* 电流通道1 10～150代表1.0A～15.0A */
            LD2_S_Cur = 0x90,  /* 电流通道2 10～150代表1.0A～15.0A */
            DFLT_V = 0x31,     /* 初始电压 20～45，代表20V～45V */
            INTER_TRG_FREQ,    /* 内触发频率 1Hz～1000Hz */
            PULSE_WIDTH,       /* 脉宽 100us～250us */
            Q_DELAY,           /* 调Ｑ延时 50us～300us */
            TRG_TYPE,          /* 触发模式 0x55内触发；0xAA外触发 */
            LD1_SW,            /* LD电流开关通道1 0x55:关LD电流；0xAA：开LD电流。上电默认关 */
            LD2_SW = 0x96,     /* LD电流开关通道2 0x55:关LD电流；0xAA：开LD电流。上电默认关 */
            Q_SW = 0x37,       /* Q脉冲开关 0x55:关Ｑ脉冲；0xAA：开Ｑ脉冲。上电默认开 */
            TEC1_SW,           /* TEC1开关 0x55：关TEC1；0xAA：开TEC1。上电默认开 */
            TEC2_SW,           /* TEC2开关 0x55：关TEC2；0xAA：开TEC2。上电默认开 */
            TEC3_SW = 0x98,    /* TEC3开关 0x55：关TEC3；0xAA：开TEC3。上电默认开 */
            TEC4_SW,           /* TEC4开关 0x55：关TEC4；0xAA：开TEC4。上电默认开 */
            TEC1_PARA = 0x3A,  /* 第1路TEC：工作温度、限压、模式 设置TEC1参数，共3个参数，每个参数2个字节：1、工作温度（150～650，代表15.0℃～65.0℃）2、限压3、模式 */
            TEC2_PARA,         /* 第2路TEC */
            TEC3_PARA = 0x9A,  /* 第3路TEC */
            TEC4_PARA,         /* 第4路TEC */
            PULSE_PARA = 0x3C, /* 子脉冲参数，共12个参数，每个参数2个字节：1、子脉冲个数（取值范围1~12）2~11、脉冲间隔1（取值范围240~660）*/


            P_UPGRADE = 0x76,          /* 在线程序升级 */

            ////////////////////////////////////////////////


            LD1_Vol = 0xC0, /* LD1电压值 206～338代表20.6V～33.8V */
            LD2_Vol,        /* LD2电压值 206～338代表20.6V～33.8V */
            LD1_HOC,        /* LD1硬件过流  10～150代表1.0A～15.0A*/
            LD2_HOC,        /* LD1硬件过流  10～150代表1.0A～15.0A*/
            LD1_SKB,        /* LD1电流设定值标定参数 */
            LD2_SKB,        /* LD2电流设定值标定参数 */
            LD1_MKB,        /* LD1电流测量值标定参数 */
            LD2_MKB,        /* LD2电流测量值标定参数 */
            PD_MKB,         /* 外部输入电平1（PD）（光功率检测值）检测值标定参数 */
            TEC1_PID,       /* TEC1的PID参数 */
            TEC2_PID,       /* TEC2的PID参数 */
            TEC3_PID,       /* TEC3的PID参数 */
            TEC4_PID,       /* TEC4的PID参数 */
            LCM,            /* 液冷模块参数 */

        }

        private enum DEV_GET_CMD_TYPE
        {
            LD1_S_Cur = 0xA0,   /* LD电流设定值通道1 */
            LD2_S_Cur = 0xF0,   /* LD电流设定值通道2 */
            DFLT_V = 0xA1,      /* 初始电压设定值 */
            INTER_TRG_FREQ,     /* 内触发频率设定值 */
            PULSE_WIDTH,        /* 脉宽设定值 */
            Q_DELAY,            /* 调Ｑ延时设定值 */
            TRG_TYPE,           /* 触发模式 */
            LD1_SW,             /* LD1开关 */
            LD2_SW = 0xF6,      /* LD2开关 */
            Q_SW = 0xA7,        /* Q脉冲设定值 */
            TEC1_SW,            /* TEC1开关 */
            TEC2_SW,            /* TEC2开关 */
            TEC3_SW = 0xF8,     /* TEC3开关 */
            TEC4_SW,            /* TEC4开关 */
            TEC1_PARA = 0xAA,   /* 第1路TEC：温度、限压、模式设定值 */
            TEC2_PARA,          /* 第2路TEC：温度、限压、模式设定值 */
            TEC3_PARA = 0xFA,   /* 第3路TEC：温度、限压、模式设定值 */
            TEC4_PARA,          /* 第4路TEC：温度、限压、模式设定值 */
            PULSE_PARA = 0xAC,  /* 子脉冲个数及脉冲间隔设定值 */
            LD1_M_Cur,          /* LD电流检测值通道1 */
            LD2_M_Cur = 0xFD,   /* LD电流检测值通道2 */
            L1_M_V = 0xAE,      /* 负载电压检测值通道1 */
            L2_M_V = 0xEE,      /* 负载电压检测值通道2 */
            PWR_TEMP = 0xAF,    /* 电源温度检测值 */
            OUT_PD = 0xC0,      /* 外部输入电平1（PD）检测值 */
            OUT_TEMP,           /* 外部输入电平2（温度）检测值 */
            TEC1_M_TEMP,        /* 第1路检测温度检测值 */
            TEC1_M_PW,          /* 第1路TEC输出功率检测值 */
            TEC2_M_TEMP,        /* 第2路检测温度检测值 */
            TEC2_M_PW,          /* 第2路TEC输出功率检测值 */
            TEC3_M_TEMP = 0x12, /* 第3路检测温度检测值 */
            TEC3_M_PW,          /* 第3路TEC输出功率检测值 */
            TEC4_M_TEMP,        /* 第4路检测温度检测值 */
            TEC4_M_PW,          /* 第4路TEC输出功率检测值 */
            WRK_STA = 0xC6,     /* 工作状态 */
            ALL_SET,            /* 所有设定参数 */
            ALL_M = 0xCF,       /* 所有检测参数 */
            UPGRADE = 0xD0,     /* 在线程序升级 */

            ////////////////////////////////////////////////
            LD1_Vol = 0x20, /* LD1电压值 206～338代表20.6V～33.8V */
            LD2_Vol,        /* LD2电压值 206～338代表20.6V～33.8V */
            LD1_HOC,        /* LD1硬件过流  10～150代表1.0A～15.0A*/
            LD2_HOC,        /* LD1硬件过流  10～150代表1.0A～15.0A*/
            LD1_SKB,        /* LD1电流设定值标定参数 */
            LD2_SKB,        /* LD2电流设定值标定参数 */
            LD1_MKB,        /* LD1电流测量值标定参数 */
            LD2_MKB,        /* LD2电流测量值标定参数 */
            PD_MKB,         /* 外部输入电平1（PD）（光功率检测值）检测值标定参数 */
            TEC1_PID,       /* TEC1的PID参数 */
            TEC2_PID,       /* TEC2的PID参数 */
            TEC3_PID,       /* TEC3的PID参数 */
            TEC4_PID,       /* TEC4的PID参数 */
            S_LCM,          /* 液冷模块设置参数 */
            M_LCM,          /* 液冷模块反馈参数 */

        }
        private void dataHandler()
        {
            while (true)
            {
                Thread.Sleep(100);
                List<PLDRev> revs = new List<PLDRev>();
                lock (locker)
                {
                    revs.AddRange(revdatas);
                    revdatas.Clear();
                }
                foreach (var item in revs)
                {
                    var d = ParseFrame(item);
                    _callback?.Invoke(d.ParamsGet, d.result);
                }
            }
        }

        private (object ParamsGet, object result) ParseFrame(PLDRev data)
        {
            PLDParams.PLDParamsFromGet paramsGet = PLDParams.PLDParamsFromGet.None;
            object result = null;

            switch ((DEV_GET_CMD_TYPE)data.cmd)
            {
                case DEV_GET_CMD_TYPE.LD1_S_Cur:
                    paramsGet = PLDParams.PLDParamsFromGet.LD1_S_Cur;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD2_S_Cur:
                    paramsGet = PLDParams.PLDParamsFromGet.LD2_S_Cur;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.DFLT_V:

                    break;
                case DEV_GET_CMD_TYPE.INTER_TRG_FREQ:
                    paramsGet = PLDParams.PLDParamsFromGet.INTER_TRG_FREQ;
                    result = data.ToUInt16;
                    break;
                case DEV_GET_CMD_TYPE.PULSE_WIDTH:
                    paramsGet = PLDParams.PLDParamsFromGet.PULSE_WIDTH;
                    result = data.ToUInt16;
                    break;
                case DEV_GET_CMD_TYPE.Q_DELAY:
                    paramsGet = PLDParams.PLDParamsFromGet.Q_DELAY;
                    result = data.ToUInt16;
                    break;
                case DEV_GET_CMD_TYPE.TRG_TYPE:
                    paramsGet = PLDParams.PLDParamsFromGet.TRG_TYPE;
                    result = data.data[0] == (byte)PLDParams.TRIGTYPE.INTER ? PLDParams.TRIGTYPE.INTER : PLDParams.TRIGTYPE.OUT;
                    break;
                case DEV_GET_CMD_TYPE.LD1_SW: paramsGet = PLDParams.PLDParamsFromGet.LD1_SW; goto SWParam;
                case DEV_GET_CMD_TYPE.LD2_SW: paramsGet = PLDParams.PLDParamsFromGet.LD2_SW; goto SWParam;
                case DEV_GET_CMD_TYPE.Q_SW: paramsGet = PLDParams.PLDParamsFromGet.Q_SW; goto SWParam;
                case DEV_GET_CMD_TYPE.TEC1_SW: paramsGet = PLDParams.PLDParamsFromGet.TEC1_SW; goto SWParam;
                case DEV_GET_CMD_TYPE.TEC2_SW: paramsGet = PLDParams.PLDParamsFromGet.TEC2_SW; goto SWParam;
                case DEV_GET_CMD_TYPE.TEC3_SW: paramsGet = PLDParams.PLDParamsFromGet.TEC3_SW; goto SWParam;
                case DEV_GET_CMD_TYPE.TEC4_SW:
                    paramsGet = PLDParams.PLDParamsFromGet.LD1_SW; goto SWParam;
                SWParam:
                    result = data.data[0] == WorkON ? 1 : 0;
                    break;
                case DEV_GET_CMD_TYPE.TEC1_PARA: paramsGet = PLDParams.PLDParamsFromGet.TEC1_PARA; goto TECParam;
                case DEV_GET_CMD_TYPE.TEC2_PARA: paramsGet = PLDParams.PLDParamsFromGet.TEC2_PARA; goto TECParam;
                case DEV_GET_CMD_TYPE.TEC3_PARA: paramsGet = PLDParams.PLDParamsFromGet.TEC3_PARA; goto TECParam;
                case DEV_GET_CMD_TYPE.TEC4_PARA:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC4_PARA; goto TECParam;
                TECParam:
                    {
                        PLDParams pLDParams = new PLDParams();
                        pLDParams.tECParams.Temp = BitConverter.ToInt16(data.data, 0) / 10.0f;
                        pLDParams.tECParams.Vol = BitConverter.ToInt16(data.data, 2) / 10.0f;
                        result = pLDParams;
                    }
                    break;
                case DEV_GET_CMD_TYPE.PULSE_PARA:
                    {
                        paramsGet = PLDParams.PLDParamsFromGet.PULSE_PARA;
                        PLDParams pLDParams = new PLDParams();
                        pLDParams.pulseParams.Num = BitConverter.ToUInt16(data.data, 0);
                        for (int i = 0; i < 11; i++)
                        {
                            pLDParams.pulseParams.Interval[i] = BitConverter.ToUInt16(data.data, 2 + 2 * i);
                        }
                        result = pLDParams;
                    }
                    break;
                case DEV_GET_CMD_TYPE.LD1_M_Cur:
                    paramsGet = PLDParams.PLDParamsFromGet.LD1_M_Cur;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD2_M_Cur:
                    paramsGet = PLDParams.PLDParamsFromGet.LD2_M_Cur;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.L1_M_V:
                    paramsGet = PLDParams.PLDParamsFromGet.L1_M_V;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.L2_M_V:
                    paramsGet = PLDParams.PLDParamsFromGet.L2_M_V;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.PWR_TEMP:
                    paramsGet = PLDParams.PLDParamsFromGet.PWR_TEMP;
                    result = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.OUT_PD:
                    paramsGet = PLDParams.PLDParamsFromGet.PWR_TEMP;
                    result = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.OUT_TEMP:
                    paramsGet = PLDParams.PLDParamsFromGet.OUT_TEMP;
                    result = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC1_M_TEMP:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC1_M_TEMP;
                    result = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC1_M_PW:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC1_M_PW;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC2_M_TEMP:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC2_M_TEMP;
                    result = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC2_M_PW:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC2_M_PW;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC3_M_TEMP:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC3_M_TEMP;
                    result = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC3_M_PW:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC3_M_PW;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC4_M_TEMP:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC4_M_TEMP;
                    result = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC4_M_PW:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC4_M_PW;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.WRK_STA:
                    paramsGet = PLDParams.PLDParamsFromGet.WRK_STA;
                    result = (UInt32)((data.data[0]<<16)| (data.data[1] << 8)|(data.data[2] << 0));
                    break;
                case DEV_GET_CMD_TYPE.ALL_SET:
                    break;
                case DEV_GET_CMD_TYPE.ALL_M:
                    break;
                case DEV_GET_CMD_TYPE.UPGRADE:
                    break;
                case DEV_GET_CMD_TYPE.LD1_Vol:
                    paramsGet = PLDParams.PLDParamsFromGet.LD1_Vol;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD2_Vol:
                    paramsGet = PLDParams.PLDParamsFromGet.LD2_Vol;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD1_HOC:
                    paramsGet = PLDParams.PLDParamsFromGet.LD1_HOC;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD2_HOC:
                    paramsGet = PLDParams.PLDParamsFromGet.LD2_HOC;
                    result = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD1_SKB: paramsGet = PLDParams.PLDParamsFromGet.LD1_SKB; goto KBparam;
                case DEV_GET_CMD_TYPE.LD2_SKB: paramsGet = PLDParams.PLDParamsFromGet.LD2_SKB; goto KBparam;
                case DEV_GET_CMD_TYPE.LD1_MKB: paramsGet = PLDParams.PLDParamsFromGet.LD1_MKB; goto KBparam;
                case DEV_GET_CMD_TYPE.LD2_MKB: paramsGet = PLDParams.PLDParamsFromGet.LD2_MKB; goto KBparam;
                case DEV_GET_CMD_TYPE.PD_MKB:
                    paramsGet = PLDParams.PLDParamsFromGet.PD_MKB; goto KBparam;
                KBparam:
                    {
                        PLDParams pLDParams = new PLDParams();
                        pLDParams.CalibCoef.k = BitConverter.ToInt16(data.data, 0) / 100.0f;
                        pLDParams.CalibCoef.b = BitConverter.ToInt16(data.data, 2) / 100.0f;
                        result = pLDParams;
                    }
                    break;
                case DEV_GET_CMD_TYPE.TEC1_PID: paramsGet = PLDParams.PLDParamsFromGet.TEC1_PID; goto PID;
                case DEV_GET_CMD_TYPE.TEC2_PID: paramsGet = PLDParams.PLDParamsFromGet.TEC2_PID; goto PID;
                case DEV_GET_CMD_TYPE.TEC3_PID: paramsGet = PLDParams.PLDParamsFromGet.TEC3_PID; goto PID;
                case DEV_GET_CMD_TYPE.TEC4_PID:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC4_PID; goto PID;
                PID:
                    {
                        PLDParams pLDParams = new PLDParams();
                        pLDParams.PIDCoef.p = BitConverter.ToInt16(data.data, 0) / 100.0f;
                        pLDParams.PIDCoef.i = BitConverter.ToInt16(data.data, 2) / 100.0f;
                        pLDParams.PIDCoef.d = BitConverter.ToInt16(data.data, 4) / 100.0f;
                        result = pLDParams;
                    }
                    break;
                case DEV_GET_CMD_TYPE.S_LCM:
                    {
                        paramsGet = PLDParams.PLDParamsFromGet.S_LCM;
                        PLDParams pLDParams = new PLDParams();

                        pLDParams.LCMSet.PwrEn = (byte)(data.data[0] == WorkON ? 1 : 0);
                        pLDParams.LCMSet.MotorEn = (byte)(data.data[1] == WorkON ? 1 : 0);
                        pLDParams.LCMSet.MotorSpeed = BitConverter.ToUInt16(data.data, 2);
                        pLDParams.LCMSet.Fan = data.data[5];
                        pLDParams.LCMSet.PwrLimit = data.data[6];
                        result = pLDParams;
                    }
                    break;
                case DEV_GET_CMD_TYPE.M_LCM:
                    {
                        paramsGet = PLDParams.PLDParamsFromGet.M_LCM;
                        PLDParams pLDParams = new PLDParams();

                        pLDParams.LCMMeasure.MotorEn = (byte)(data.data[1] == WorkON ? 1 : 0);
                        pLDParams.LCMMeasure.MotorSpeed = BitConverter.ToUInt16(data.data, 1);
                        pLDParams.LCMMeasure.DCVoltage = BitConverter.ToUInt16(data.data, 3) / 10.0f;
                        pLDParams.LCMMeasure.DCCurrent = data.data[6] / 10.0f;
                        pLDParams.LCMMeasure.Temp = BitConverter.ToInt16(data.data, 6) / 10.0f;
                        pLDParams.LCMMeasure.PwrLimit = data.data[7];
                        result = pLDParams;
                    }
                    break;
                default:
                    break;
            }


            return (paramsGet, result);
        }
        private byte SumCRC(byte[] data, int len)
        {
            byte sum = 0;
            for (int i = 0; i < len; i++)
            {
                sum += data[i];
            }
            return sum;
        }

        public void AddData(byte[] data)
        {
            List<byte> receiveBufferTemp = new List<byte>();

            receiveBufferTemp.AddRange(data);
            receiveBufferTemp.AddRange(data);
            remainData.Clear();
            int rev_cnt = receiveBufferTemp.Count;
            if (rev_cnt < 8)
            {
                remainData.AddRange(receiveBufferTemp);
                return;
            }
            int take_data_cnt = 0;
            for (int i = 0; i < rev_cnt - 3; i++)
            {

                int remain_cnt = rev_cnt - i;

                if (remain_cnt >= 9 &&
                    BitConverter.ToUInt16(receiveBufferTemp.ToArray(), i) == CMD_HEAD &&
                    receiveBufferTemp[i + 2] == MCU_ID &&
                    receiveBufferTemp[i + 3] == PC_ID)
                {

                    int dataLen = receiveBufferTemp[i + 5];
                    int packetLen = dataLen + 9;

                    if (BitConverter.ToUInt16(receiveBufferTemp.ToArray(), i + dataLen + 7) == CMD_TAIL &&
                        receiveBufferTemp[i + dataLen + 6] == SumCRC(receiveBufferTemp.Skip(i+6).Take(dataLen).ToArray(), dataLen)
                        )
                    {
                        PLDRev revData = new PLDRev();
                        revData.rawData.AddRange(receiveBufferTemp.Skip(i).Take(packetLen));
                        revData.time = DateTime.Now;
                        lock (locker)
                        {
                            revdatas.Add(revData);
                        }
                        i += packetLen - 1;
                        take_data_cnt = i;
                    }
                }

            }
            remainData.AddRange(receiveBufferTemp.Skip(take_data_cnt));

        }

        public List<byte> CreateFrame(object paramsSet, params byte[] data)
        {
            List<byte> frame = new List<byte>();
           
            if (paramsSet as PLDParams.PLDParamsToSet? == null)
            {
                return frame;
            }
            byte cmd = 0;
            var a = (PLDParams.PLDParamsToSet)paramsSet;
            switch (a)
            {
                case PLDParams.PLDParamsToSet.LD1_S_Cur:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD1_S_Cur;
                    break;
                case PLDParams.PLDParamsToSet.LD2_S_Cur:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD2_S_Cur;
                    break;
                case PLDParams.PLDParamsToSet.DFLT_V:
                    cmd = (byte)DEV_SET_CMD_TYPE.DFLT_V;
                    break;
                case PLDParams.PLDParamsToSet.INTER_TRG_FREQ:
                    cmd = (byte)DEV_SET_CMD_TYPE.INTER_TRG_FREQ;
                    break;
                case PLDParams.PLDParamsToSet.PULSE_WIDTH:
                    cmd = (byte)DEV_SET_CMD_TYPE.PULSE_WIDTH;
                    break;
                case PLDParams.PLDParamsToSet.Q_DELAY:
                    cmd = (byte)DEV_SET_CMD_TYPE.Q_DELAY;
                    break;
                case PLDParams.PLDParamsToSet.TRG_TYPE:
                    cmd = (byte)DEV_SET_CMD_TYPE.TRG_TYPE;
                    break;
                case PLDParams.PLDParamsToSet.LD1_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD1_SW;
                    break;
                case PLDParams.PLDParamsToSet.LD2_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD2_SW;
                    break;
                case PLDParams.PLDParamsToSet.Q_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.Q_SW;
                    break;
                case PLDParams.PLDParamsToSet.TEC1_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC1_SW;
                    break;
                case PLDParams.PLDParamsToSet.TEC2_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC2_SW;
                    break;
                case PLDParams.PLDParamsToSet.TEC3_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC3_SW;
                    break;
                case PLDParams.PLDParamsToSet.TEC4_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC4_SW;
                    break;
                case PLDParams.PLDParamsToSet.TEC1_PARA:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC1_PARA;
                    break;
                case PLDParams.PLDParamsToSet.TEC2_PARA:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC2_PARA;
                    break;
                case PLDParams.PLDParamsToSet.TEC3_PARA:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC3_PARA;
                    break;
                case PLDParams.PLDParamsToSet.TEC4_PARA:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC4_PARA;
                    break;
                case PLDParams.PLDParamsToSet.PULSE_PARA:
                    cmd = (byte)DEV_SET_CMD_TYPE.PULSE_PARA;
                    break;
                case PLDParams.PLDParamsToSet.P_UPGRADE:
                    cmd = (byte)DEV_SET_CMD_TYPE.P_UPGRADE;
                    break;
                case PLDParams.PLDParamsToSet.LD1_Vol:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD1_Vol;
                    break;
                case PLDParams.PLDParamsToSet.LD2_Vol:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD2_Vol;
                    break;
                case PLDParams.PLDParamsToSet.LD1_HOC:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD1_HOC;
                    break;
                case PLDParams.PLDParamsToSet.LD2_HOC:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD2_HOC;
                    break;
                case PLDParams.PLDParamsToSet.LD1_SKB:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD1_SKB;
                    break;
                case PLDParams.PLDParamsToSet.LD2_SKB:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD2_SKB;
                    break;
                case PLDParams.PLDParamsToSet.LD1_MKB:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD1_MKB;
                    break;
                case PLDParams.PLDParamsToSet.LD2_MKB:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD2_MKB;
                    break;
                case PLDParams.PLDParamsToSet.PD_MKB:
                    cmd = (byte)DEV_SET_CMD_TYPE.PD_MKB;
                    break;
                case PLDParams.PLDParamsToSet.TEC1_PID:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC1_PID;
                    break;
                case PLDParams.PLDParamsToSet.TEC2_PID:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC2_PID;
                    break;
                case PLDParams.PLDParamsToSet.TEC3_PID:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC3_PID;
                    break;
                case PLDParams.PLDParamsToSet.TEC4_PID:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC4_PID;
                    break;
                case PLDParams.PLDParamsToSet.LCM:
                    cmd = (byte)DEV_SET_CMD_TYPE.LCM;
                    break;
                default:
                    break;
            }

            frame.Add(CMD_HEAD >> 8);
            frame.Add(CMD_HEAD & 0xff);
            frame.Add(PC_ID);
            frame.Add(MCU_ID);
            frame.Add(cmd);
            frame.Add((byte)data.Length);//dataLen
            frame.Add(SumCRC(data, data.Length));//CRC
            frame.Add(CMD_TAIL >> 8);
            frame.Add(CMD_TAIL & 0xff);

            return frame;
        }

        public List<byte> CreateQueryFrame(object paramsQuery)
        {
            List<byte> frame = new List<byte>();
            if (paramsQuery as PLDParams.PLDParamsToQuery? ==null)
            {
                return frame;
            }
            var a = (PLDParams.PLDParamsToQuery)paramsQuery;
            byte cmd = 0;
            switch (a)
            {
                case PLDParams.PLDParamsToQuery.Cur:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.Cur;
                    break;
                case PLDParams.PLDParamsToQuery.DFLT_V:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.DFLT_V;
                    break;
                case PLDParams.PLDParamsToQuery.INTER_TRG_FREQ:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.INTER_TRG_FREQ;
                    break;
                case PLDParams.PLDParamsToQuery.PULSE_WIDTH:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.PULSE_WIDTH;
                    break;
                case PLDParams.PLDParamsToQuery.Q_DELAY:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.Q_DELAY;
                    break;
                case PLDParams.PLDParamsToQuery.TRG_TYPE:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TRG_TYPE;
                    break;
                case PLDParams.PLDParamsToQuery.LD1_S_Cur:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD1_S_Cur;
                    break;
                case PLDParams.PLDParamsToQuery.LD2_S_Cur:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD2_S_Cur;
                    break;
                case PLDParams.PLDParamsToQuery.Q_SW:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.Q_SW;
                    break;
                case PLDParams.PLDParamsToQuery.TEC1_SW:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC1_SW;
                    break;
                case PLDParams.PLDParamsToQuery.TEC2_SW:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC2_SW;
                    break;
                case PLDParams.PLDParamsToQuery.TEC3_SW:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC3_SW;
                    break;
                case PLDParams.PLDParamsToQuery.TEC4_SW:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC4_SW;
                    break;
                case PLDParams.PLDParamsToQuery.TEC1_PARA:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC1_PARA;
                    break;
                case PLDParams.PLDParamsToQuery.TEC2_PARA:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC2_PARA;
                    break;
                case PLDParams.PLDParamsToQuery.TEC3_PARA:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC3_PARA;
                    break;
                case PLDParams.PLDParamsToQuery.TEC4_PARA:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC4_PARA;
                    break;
                case PLDParams.PLDParamsToQuery.PULSE_PARA:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.PULSE_PARA;
                    break;
                case PLDParams.PLDParamsToQuery.LD1_M_Cur:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD1_M_Cur;
                    break;
                case PLDParams.PLDParamsToQuery.LD2_M_Cur:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD2_M_Cur;
                    break;
                case PLDParams.PLDParamsToQuery.L1_M_V:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.L1_M_V;
                    break;
                case PLDParams.PLDParamsToQuery.L2_M_V:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.L2_M_V;
                    break;
                case PLDParams.PLDParamsToQuery.PWR_TEMP:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.PWR_TEMP;
                    break;
                case PLDParams.PLDParamsToQuery.OUT_PD:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.OUT_PD;
                    break;
                case PLDParams.PLDParamsToQuery.OUT_TEMP:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.OUT_TEMP;
                    break;
                case PLDParams.PLDParamsToQuery.TEC1_M_TEMP:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC1_M_TEMP;
                    break;
                case PLDParams.PLDParamsToQuery.TEC1_M_PW:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC1_M_PW;
                    break;
                case PLDParams.PLDParamsToQuery.TEC2_M_TEMP:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC2_M_TEMP;
                    break;
                case PLDParams.PLDParamsToQuery.TEC2_M_PW:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC2_M_PW;
                    break;
                case PLDParams.PLDParamsToQuery.TEC3_M_TEMP:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC3_M_TEMP;
                    break;
                case PLDParams.PLDParamsToQuery.TEC3_M_PW:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC3_M_PW;
                    break;
                case PLDParams.PLDParamsToQuery.TEC4_M_TEMP:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC4_M_TEMP;
                    break;
                case PLDParams.PLDParamsToQuery.TEC4_M_PW:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC4_M_PW;
                    break;
                case PLDParams.PLDParamsToQuery.WRK_STA:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.WRK_STA;
                    break;
                case PLDParams.PLDParamsToQuery.ALL_SET:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.ALL_SET;
                    break;
                case PLDParams.PLDParamsToQuery.ALL_M:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.ALL_M;
                    break;
                case PLDParams.PLDParamsToQuery.LD1_SW:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD1_SW;
                    break;
                case PLDParams.PLDParamsToQuery.LD2_SW:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD2_SW;
                    break;
                case PLDParams.PLDParamsToQuery.LD1_Vol:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD1_Vol;
                    break;
                case PLDParams.PLDParamsToQuery.LD2_Vol:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD2_Vol;
                    break;
                case PLDParams.PLDParamsToQuery.LD1_HOC:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD1_HOC;
                    break;
                case PLDParams.PLDParamsToQuery.LD2_HOC:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD2_HOC;
                    break;
                case PLDParams.PLDParamsToQuery.LD1_SKB:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD1_SKB;
                    break;
                case PLDParams.PLDParamsToQuery.LD2_SKB:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD2_SKB;
                    break;
                case PLDParams.PLDParamsToQuery.LD1_MKB:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD1_MKB;
                    break;
                case PLDParams.PLDParamsToQuery.LD2_MKB:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.LD2_MKB;
                    break;
                case PLDParams.PLDParamsToQuery.PD_MKB:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.PD_MKB;
                    break;
                case PLDParams.PLDParamsToQuery.TEC1_PID:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC1_PID;
                    break;
                case PLDParams.PLDParamsToQuery.TEC2_PID:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC2_PID;
                    break;
                case PLDParams.PLDParamsToQuery.TEC3_PID:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC3_PID;
                    break;
                case PLDParams.PLDParamsToQuery.TEC4_PID:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.TEC4_PID;
                    break;
                case PLDParams.PLDParamsToQuery.S_LCM:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.S_LCM;
                    break;
                case PLDParams.PLDParamsToQuery.M_LCM:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.M_LCM;
                    break;
                default:
                    break;
            }
            frame.Add(CMD_HEAD>>8);
            frame.Add(CMD_HEAD &0xff);
            frame.Add(PC_ID);
            frame.Add(MCU_ID);
            frame.Add(cmd);
            frame.Add(0);//dataLen
            frame.Add(0);//CRC
            frame.Add(CMD_TAIL>>8);
            frame.Add(CMD_TAIL &0xff);

            return frame;
        }

        private Action<object, object> _callback;
        public void SetCallback(Action<object, object> callback)
        {
            _callback = callback;
        }
    }
}
