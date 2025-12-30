using Prism.Mvvm;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wpfApp.Common.CtrlProtocol;
using static SkiaSharp.HarfBuzz.SKShaper;
using static wpfApp.Common.PLDProtocol.PLDParams;
using static wpfApp.Common.PLDProtocol.PLDParams.MeasureParam;
using static wpfApp.ViewModels.PLDMainViewModel;

namespace wpfApp.Common.PLDProtocol
{

   
 

    public class PLDParams
    {

        public DateTime time { get; set; }

        public MeasureParam MeasureParams { get; set; } =   new MeasureParam();
        public SetParam SetParams { get; set; } = new SetParam();

        public class MeasureParam
        {
            public UInt32 Status { get; set; }
            public string Version { get; set; }

            public bool ParamSaveStatus { get; set; }

            public UpgradeResult UpgradeResult { get; set; }

            public UpgradeParam UpgradeParams { get; set; }

            public class UpgradeParam
            {
                public UpgradeStatus Status { get; set; }
                public UInt16 CurrIdx { get; set; }
            }

            public class LDParam
            {
                public float Curr { get; set; }
                public float Vol { get; set; }
                public float Temp { get; set; }
            }

            public class TECParam
            {
                public float Curr { get; set; }
                public float Power { get; set; }
                public float Temp { get; set; }
            }

            public class PDParam
            {
                public float Power { get; set; }
                public float Temp { get; set; }
            }

            public class LCMParam
            {
                public WorkType IsIsMotorWork { get; set; }
                public float Vol { get; set; }
                public float Curr { get; set; }
                public float Power { get; set; }
                public float MotorSpeed { get; set; }
                public float Temp { get; set; }
            }
            public class OtherInfo
            {
                public float PwrTemp { get; set; }
                public float SysCurr { get; set; }
                public float SysVol { get; set; }
            }
            public OtherInfo OtherInfos { get; set; }
            public LCMParam LCMParams { get; set; }

            public PDParam PDParams { get; set; }
            public List<TECParam> TECParams { get; set; }

            public List<LDParam> LDParams { get; set; }

            public MeasureParam()
            {
                UpgradeParams = new UpgradeParam();
                OtherInfos = new OtherInfo();
                LCMParams = new LCMParam();
                PDParams = new PDParam();
                TECParams = new List<TECParam>() { new TECParam(), new TECParam(), new TECParam(), new TECParam() };
                LDParams = new List<LDParam>() { new LDParam(), new LDParam() };
            }
        }

        




        public class SetParam
        {



            public TrigType TrigType { get; set; }

            public PulseType PulseType { get; set; }

            public CalibParam CalibPD { get; set; } = new CalibParam();

            public class CalibParam
            {
                public float K { get; set; }
                public float B { get; set; }
            }

            public class PIDCalibParam
            {
                public float P { get; set; }
                public float I { get; set; }
                public float D { get; set; }
            }

            public class LDParam
            {
                public WorkType WorkType { get; set; }
                public float Curr { get; set; }
                public float Vol { get; set; }
                public float Temp { get; set; }
                public float MaxCurr { get; set; }
                public float HOC { get; set; }

                public CalibParam CalibMeasure { get; set; } = new CalibParam();

                public CalibParam CalibSet { get; set; } = new CalibParam();
            }

            public class TECParam
            {
                public WorkType WorkType { get; set; }
                public float Curr { get; set; }
                public float Vol { get; set; }
                public Int16 Mode { get; set; }//模式 未使用
                public float Power { get; set; }
                public float Temp { get; set; }
                public PIDCalibParam PID { get; set; } = new PIDCalibParam();
            }

            public class TQParam
            {
                public WorkType WorkType { get; set; }
                public float Delay { get; set; }
              
            }
            public class LCMParam
            {
                public WorkType IsPowerOn { get; set; }

                public WorkType IsIsMotorWork { get; set; }

                public WorkType FanSpeed { get; set; }

                public UInt16 MotorSpeed { get; set; }
                public byte PwrLimit { get; set; }
               
            }
            public class PulseParam
            {

                public UInt16 Width { get; set; }
                public UInt16 Freq { get; set; }
                public UInt16 Num { get; set; }
                public List<UInt16> Interval { get; set; } = new List<UInt16>(11);

            }
            public PulseParam PulseParams { get; set; }
            public TQParam TQParams { get; set; }
            public LCMParam LCMParams { get; set; }          
            public List<TECParam> TECParams { get; set; }
            public List<LDParam> LDParams { get; set; }

            public Upgrade UpgradeParams { get; set; }

            public class Upgrade
            {
                public UInt16 CurrIdx { get; set; }
                public UInt16 TotalPaketNum { get; set; }

                public List<byte> Data { get; set; } = [];
            }

            public SetParam()
            {
                PulseParams = new PulseParam();
                for (int i = 0; i < 11; i++)
                {
                    PulseParams.Interval.Add(0);
                }
                TQParams = new TQParam();
                LCMParams = new LCMParam();
                TECParams = new List<TECParam>() { new TECParam(), new TECParam(), new TECParam(), new TECParam() };
                LDParams = new List<LDParam>() { new LDParam(), new LDParam() };
                UpgradeParams = new Upgrade();
            }
        }

        

        public class PulseParams
        {
            public UInt16 Num;
            public UInt16[] Interval = new UInt16[11];
        }

        
        public class TECParams
        {
            public float Temp;
            public float Vol;
            public UInt16 Mode;
        }

       
    

 
        public class LCMSetParams
        {
            public byte PwrEn;        // 电泵供电开关
            public byte MotorEn;      // 电机开关
            public UInt16 MotorSpeed; // 转速 1rpm
            public byte Fan;          // 风扇 100代表100占空比
            public byte PwrLimit;     // 功率限制 W
        }

     

       
     


        [Flags]
        public enum StatusFlags : UInt32
        {
            
            LD1_UC      = 1 << 0,       // LD1驱动欠压
            LD1_OC      = 1 << 1,       // LD1驱动过压
            LD2_UC      = 1 << 2,       // LD2驱动欠压
            LD2_OC      = 1 << 3,       // LD2驱动过压           
            NTC1_ERR    = 1 << 4,       // 热敏电阻1异常
            NTC2_ERR    = 1 << 5,       // 热敏电阻2异常
            NTC3_ERR    = 1 << 6,       // 热敏电阻3异常
            NTC4_ERR    = 1 << 7,      // 热敏电阻4异常
            TEC1_UT     = 1 << 8,       // TEC1欠温
            TEC1_OT     = 1 << 9,       // TEC1过温
            TEC2_UT     = 1 << 10,       // TEC2欠温
            TEC2_OT     = 1 << 11,       // TEC2过温   
            TEC3_UT     = 1 << 12,       // TEC3欠温
            TEC3_OT     = 1 << 13,       // TEC3过温
            TEC4_UT     = 1 << 14,       // TEC4欠温
            TEC4_OT     = 1 << 15,       // TEC4过温   
            EPPROM_ERR  = 1 << 16,       // 参数存储错误
            PWR_OT      = 1 << 17,       // 电源过热
            PWR_ERR     = 1 << 18,       // 电路故障
            TEC1_SW     = 1 << 19,      // TEC1 开关 1：开启; 0：关闭
            TEC2_SW     = 1 << 20,      // TEC2 开关 1：开启; 0：关闭
            TEC3_SW     = 1 << 21,      // TEC3 开关 1：开启; 0：关闭
            TEC4_SW     = 1 << 22,      // TEC4 开关 1：开启; 0：关闭
            Q_SW        = 1 << 23,      // 调Q 开关 1：开启; 0：关闭
            LD1_SW      = 1 << 24,      // LD1 开关 1：开启; 0：关闭
            LD2_SW      = 1 << 25,      // LD2 开关 1：开启; 0：关闭
            TRQ_Type    = 1 << 26,      // 触发状态 1：外触发; 0：内触发
            Pulse_Type  = 1 << 27,      // 脉冲类型 1：定频 0：变频
        }


        public class Status
        {
            public StatusFlags statusFlags;
            public string Name;
        }
        static public Status[] ErrStatus =
        {

            new Status() { Name= "LD1驱动欠流 ",statusFlags=StatusFlags.LD1_UC },
            new Status() { Name= "LD1驱动过流 ",statusFlags=StatusFlags.LD1_OC },
            new Status() { Name= "LD2驱动欠流 ",statusFlags=StatusFlags.LD2_UC },
            new Status() { Name= "LD2驱动过流 ",statusFlags=StatusFlags.LD2_OC },
            new Status() { Name= "热敏电阻1异常 ",statusFlags=StatusFlags.NTC1_ERR },
            new Status() { Name= "热敏电阻2异常 ",statusFlags=StatusFlags.NTC2_ERR },
            new Status() { Name= "热敏电阻3异常 ",statusFlags=StatusFlags.NTC3_ERR },
            new Status() { Name= "热敏电阻4异常 ",statusFlags=StatusFlags.NTC4_ERR },
            new Status() { Name= "TEC1欠温 ",statusFlags=StatusFlags.TEC1_UT },
            new Status() { Name= "TEC1过温 ",statusFlags=StatusFlags.TEC1_OT },
            new Status() { Name= "TEC2欠温 ",statusFlags=StatusFlags.TEC2_UT },
            new Status() { Name= "TEC2过温 ",statusFlags=StatusFlags.TEC2_OT },
            new Status() { Name= "TEC3欠温 ",statusFlags=StatusFlags.TEC3_UT },
            new Status() { Name= "TEC3过温 ",statusFlags=StatusFlags.TEC3_OT },
            new Status() { Name= "TEC4欠温 ",statusFlags=StatusFlags.TEC4_UT },
            new Status() { Name= "TEC4过温 ",statusFlags=StatusFlags.TEC4_OT },


            new Status() { Name= "参数存储错误 ",statusFlags=StatusFlags.EPPROM_ERR },
            new Status() { Name= "电源过热 ",statusFlags=StatusFlags.PWR_OT },
            new Status() { Name= "电路故障 ",statusFlags=StatusFlags.PWR_ERR },
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
            new Status() { Name= "外触发 ",statusFlags=StatusFlags.TRQ_Type },
            new Status() { Name= "定频 ",statusFlags=StatusFlags.Pulse_Type },
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
            TEC1_M_Cur,         /* 查询第1路TEC输出电流检测值 */
            TEC2_M_Cur,         /* 查询第2路TEC输出电流检测值 */
            TEC3_M_Cur,         /* 查询第3路TEC输出电流检测值 */
            TEC4_M_Cur,         /* 查询第4路TEC输出电流检测值 */
            WRK_STA = 0xC6,     /* 查询工作状态 */
            ALL_SET,            /* 查询所有设定参数 */
            ALL_M = 0xCF,       /* 查询所有检测参数 */
            Upgrade = 0xD0,     /* 在线程序升级 */

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
            SaveParam,/* 参数保存 */
            PulseType,/* 触发频率模式 */
            BootMode ,/*BOOT模式*/
            UpgradeResult,/* 上次升级结果 */
        }
      

        public enum PLDParamsToSet
        {
            LD1_S_Cur = 0x30,  /* 电流通道1 10～150代表1.0A～15.0A */
            LD2_S_Cur = 0x90,  /* 电流通道2 10～150代表1.0A～15.0A */
            DFLT_V = 0x31,     /* 初始电压 20～45，代表20V～45V */
            INTER_TRG_FREQ,    /* 内触发频率 1Hz～1000Hz */
            PULSE_WIDTH,       /* 脉宽 200us～240us */
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


            Upgrade = 0x76,          /* 在线程序升级 */

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
            SaveParam,           /* 参数保存 */
            PulseType,/* 触发频率模式 */
            ClearErr,/* 清空错误 */
            SetVersion,
        }

        public enum PLDParamsToQuery
        {
            Cur,         /* 电流设定值 */
            DFLT_V,             /* 初始电压设定值 */
            INTER_TRG_FREQ,     /* 内触发频率设定值 */
            PULSE_WIDTH , /* 脉宽设定值 */
            Q_DELAY,            /* 调Ｑ延时设定值 */
            TRG_TYPE,           /* 触发模式 */
            LD1_S_Cur,          /* LD电流设定值通道1 */
            LD2_S_Cur ,   /* LD电流设定值通道2 */
            Q_SW ,        /* Q开关 */
            TEC1_SW,            /* TEC1开关 */
            TEC2_SW,            /* TEC2开关 */
            TEC3_SW,     /* TEC3开关 */
            TEC4_SW,            /* TEC4开关 */
            TEC1_PARA,   /* 第1路TEC：温度、限压、模式设定值 */
            TEC2_PARA,          /* 第2路TEC：温度、限压、模式设定值 */
            TEC3_PARA,   /* 第3路TEC：温度、限压、模式设定值 */
            TEC4_PARA,          /* 第4路TEC：温度、限压、模式设定值 */
            PULSE_PARA,  /* 子脉冲个数及脉冲间隔设定值 */

            LD1_SW, /* LD1开关 */
            LD2_SW, /* LD2开关 */

            LD1_Vol, /* LD1电压值 206～338代表20.6V～33.8V */
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
            UpgradeResult,  /* 上次升级结果 */
            ALL_SET,        /* 所有设定参数 */


            LD1_M_Cur,          /* LD电流检测值通道1 */
            LD2_M_Cur,   /* LD电流检测值通道2 */
            L1_M_V,      /* 负载电压检测值通道1 */
            L2_M_V,      /* 负载电压检测值通道2 */
            PWR_TEMP,    /* 电源温度检测值 */
            OUT_PD,             /* 外部输入电平1（PD）检测值 */
            OUT_TEMP,           /* 外部输入电平2（温度）检测值 */
            TEC1_M_TEMP,        /* 第1路检测温度检测值 */
            TEC1_M_PW,   /* 第1路TEC输出功率检测值 */
            TEC2_M_TEMP,        /* 第2路检测温度检测值 */
            TEC2_M_PW,          /* 第2路TEC输出功率检测值 */
            TEC3_M_TEMP, /* 第3路检测温度检测值 */
            TEC3_M_PW,   /* 第3路TEC输出功率检测值 */
            TEC4_M_TEMP,        /* 第4路检测温度检测值 */
            TEC4_M_PW,          /* 第4路TEC输出功率检测值 */
            TEC1_M_Cur,         /* 查询第1路TEC输出电流检测值 */
            TEC2_M_Cur,         /* 查询第2路TEC输出电流检测值 */
            TEC3_M_Cur,         /* 查询第3路TEC输出电流检测值 */
            TEC4_M_Cur,         /* 查询第4路TEC输出电流检测值 */
            M_LCM,          /* 液冷模块反馈参数 */
            WRK_STA,         /* 工作状态 */            
            ALL_M,          /* 所有检测参数 */
            PulseType,/* 触发频率模式 */
            Upgrade,
        }
        public enum TrigType
        {
            INTER = 0x55,
            OUT = 0xAA
        }
       
        public enum WorkType
        {
            OFF = 0x55,
            ON = 0xAA
        }
        public enum PulseType
        {
            SPWM = 0x55,
            NOR = 0xAA
        }


        public enum UpgradeStatus
        {
            IDLE,
            RUNNING,
            LAST_DONE,
            LAST_CHECK_ERR,
            LAST_FAILED,
            CUR_DONE,
            CUR_CHECK_ERR,
            CUR_FALIED
        }

        public enum UpgradeResult
        {
            NONE = 0,
            Pending = 1,
            Applied = 2,
            Failed = 3,
            BootMode = 4
        }
    }

  
    public class Protocol_X004_001 : ICommunicationProtocol2
    {


        public Protocol_X004_001()
        {
            Thread thread = new Thread(new ThreadStart(() => { this.dataHandler(); }));
            thread.Start();
           
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

        private const UInt16 CMD_HEAD = 0x545A;
        private const UInt16 CMD_TAIL = 0xFE5A;
        private const byte PC_ID = 0x14;
        private const byte MCU_ID = 0x1c;

 


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
            TEC1_M_Cur,         /* 查询第1路TEC输出电流检测值 */
            TEC2_M_Cur,         /* 查询第2路TEC输出电流检测值 */
            TEC3_M_Cur,         /* 查询第3路TEC输出电流检测值 */
            TEC4_M_Cur,         /* 查询第4路TEC输出电流检测值 */

            WRK_STA = 0x73, /* 工作状态 */
            ALL_SET,        /* 所有设定参数 */
            ALL_M,          /* 所有检测参数 */

            Upgrade = 0x77,/* 查询升级程序状态 */
            PulseType = 0x79, /* 查询脉冲模式 */
            LD1_SW = 0x7A, /* LD1开关 */
            LD2_SW = 0x7B, /* LD2开关 */

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
            UpgradeResult=0x13,/* 上次升级结果 */
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


            Upgrade = 0x76,          /* 在线程序升级 */
            PulseType = 0x78,/* 设置触发频率模式 */
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
            SaveParam = 0x10,/* 参数保存 */
            ClearErr =0x11,
            SetVersion,
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
            TEC1_M_Cur,         /* 查询第1路TEC输出电流检测值 */
            TEC2_M_Cur,         /* 查询第2路TEC输出电流检测值 */
            TEC3_M_Cur,         /* 查询第3路TEC输出电流检测值 */
            TEC4_M_Cur,         /* 查询第4路TEC输出电流检测值 */
            WRK_STA = 0xC6,     /* 工作状态 */
            ALL_SET,            /* 所有设定参数 */
            ALL_M = 0xCF,       /* 所有检测参数 */
            Upgrade = 0xD0,     /* 在线程序升级 */
            PulseType,/* 频率触发模式 */
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
            SaveParam = 0xE0,/* 参数保存 */
            UpgradeResult = 0xE3,/* 上次升级结果 */
            BootMode = 0xE4,/*BOOT模式*/
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
            PLDParams p = new PLDParams();
            int idx = 0;
            switch ((DEV_GET_CMD_TYPE)data.cmd)
            {
                case DEV_GET_CMD_TYPE.LD1_S_Cur:
                    paramsGet = PLDParams.PLDParamsFromGet.LD1_S_Cur;
                    p.SetParams.LDParams[0].Curr = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD2_S_Cur:
                    paramsGet = PLDParams.PLDParamsFromGet.LD2_S_Cur;
                    p.SetParams.LDParams[1].Curr  = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.DFLT_V:

                    break;
                case DEV_GET_CMD_TYPE.INTER_TRG_FREQ:
                    paramsGet = PLDParams.PLDParamsFromGet.INTER_TRG_FREQ;
                    p.SetParams.PulseParams.Freq = data.ToUInt16;
                    break;
                case DEV_GET_CMD_TYPE.PULSE_WIDTH:
                    paramsGet = PLDParams.PLDParamsFromGet.PULSE_WIDTH;
                    p.SetParams.PulseParams.Width = data.ToUInt16;
                    break;
                case DEV_GET_CMD_TYPE.Q_DELAY:
                    paramsGet = PLDParams.PLDParamsFromGet.Q_DELAY;
                    p.SetParams.TQParams.Delay = data.ToUInt16;
                    break;
                case DEV_GET_CMD_TYPE.TRG_TYPE:
                    paramsGet = PLDParams.PLDParamsFromGet.TRG_TYPE;                    
                    p.SetParams.TrigType = data.data[0] == (byte)PLDParams.TrigType.INTER ? PLDParams.TrigType.INTER : PLDParams.TrigType.OUT;
                    break;
                case DEV_GET_CMD_TYPE.LD1_SW: 
                    paramsGet = PLDParams.PLDParamsFromGet.LD1_SW;
                    p.SetParams.LDParams[0].WorkType = data.data[0] == (byte)PLDParams.WorkType.ON ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.LD2_SW: paramsGet = PLDParams.PLDParamsFromGet.LD2_SW;
                    p.SetParams.LDParams[1].WorkType = data.data[0] == (byte)PLDParams.WorkType.ON ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.Q_SW: paramsGet = PLDParams.PLDParamsFromGet.Q_SW;
                    p.SetParams.TQParams.WorkType = data.data[0] == (byte)PLDParams.WorkType.ON ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.TEC1_SW: paramsGet = PLDParams.PLDParamsFromGet.TEC1_SW;
                    p.SetParams.TECParams[0].WorkType = data.data[0] == (byte)PLDParams.WorkType.ON ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.TEC2_SW: paramsGet = PLDParams.PLDParamsFromGet.TEC2_SW;
                    p.SetParams.TECParams[1].WorkType = data.data[0] == (byte)PLDParams.WorkType.ON ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.TEC3_SW: paramsGet = PLDParams.PLDParamsFromGet.TEC3_SW;
                    p.SetParams.TECParams[2].WorkType = data.data[0] == (byte)PLDParams.WorkType.ON ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.TEC4_SW:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC4_SW; 
                    p.SetParams.TECParams[3].WorkType = data.data[0] == (byte)PLDParams.WorkType.ON ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.TEC1_PARA: paramsGet = PLDParams.PLDParamsFromGet.TEC1_PARA;idx = 0; goto TECParam;
                case DEV_GET_CMD_TYPE.TEC2_PARA: paramsGet = PLDParams.PLDParamsFromGet.TEC2_PARA; idx = 1; goto TECParam;
                case DEV_GET_CMD_TYPE.TEC3_PARA: paramsGet = PLDParams.PLDParamsFromGet.TEC3_PARA; idx = 2; goto TECParam;
                case DEV_GET_CMD_TYPE.TEC4_PARA:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC4_PARA; idx = 3; goto TECParam; 
                TECParam:
                    {
                        p.SetParams.TECParams[idx].Temp  = BitConverter.ToInt16(data.data, 0) / 10.0f;
                        p.SetParams.TECParams[idx].Vol = BitConverter.ToInt16(data.data, 2) / 10.0f;
                    }
                    break;
                case DEV_GET_CMD_TYPE.PULSE_PARA:
                    {
                        paramsGet = PLDParams.PLDParamsFromGet.PULSE_PARA;
                        p.SetParams.PulseParams.Num = BitConverter.ToUInt16(data.data, 0);
                        for (int i = 0; i < 11; i++)
                        {
                            p.SetParams.PulseParams.Interval[i] = BitConverter.ToUInt16(data.data, 2 + 2 * i);
                        }
                    }
                    break;
                case DEV_GET_CMD_TYPE.LD1_M_Cur:
                    paramsGet = PLDParams.PLDParamsFromGet.LD1_M_Cur;
                    p.MeasureParams.LDParams[0].Curr = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD2_M_Cur:
                    paramsGet = PLDParams.PLDParamsFromGet.LD2_M_Cur;
                    p.MeasureParams.LDParams[1].Curr = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.L1_M_V:
                    paramsGet = PLDParams.PLDParamsFromGet.L1_M_V;
                    p.MeasureParams.LDParams[0].Vol = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.L2_M_V:
                    paramsGet = PLDParams.PLDParamsFromGet.L2_M_V;
                    p.MeasureParams.LDParams[1].Vol = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.PWR_TEMP:
                    paramsGet = PLDParams.PLDParamsFromGet.PWR_TEMP;
                    p.MeasureParams.OtherInfos.PwrTemp = data.ToSFloat;
                   
                    break;
                case DEV_GET_CMD_TYPE.OUT_PD:
                    paramsGet = PLDParams.PLDParamsFromGet.OUT_PD;
                    p.MeasureParams.PDParams.Power = data.ToUFloat;
                    
                    break;
                case DEV_GET_CMD_TYPE.OUT_TEMP:
                    paramsGet = PLDParams.PLDParamsFromGet.OUT_TEMP;
                    p.MeasureParams.PDParams.Temp = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC1_M_TEMP:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC1_M_TEMP;
                    p.MeasureParams.TECParams[0].Temp = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC1_M_PW:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC1_M_PW;
                    p.MeasureParams.TECParams[0].Power = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC2_M_TEMP:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC2_M_TEMP;
                    p.MeasureParams.TECParams[1].Temp= data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC2_M_PW:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC2_M_PW;
                    p.MeasureParams.TECParams[1].Power = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC3_M_TEMP:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC3_M_TEMP;
                    p.MeasureParams.TECParams[2].Temp = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC3_M_PW:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC3_M_PW;
                    p.MeasureParams.TECParams[2].Power = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC4_M_TEMP:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC4_M_TEMP;
                    p.MeasureParams.TECParams[3].Temp = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC4_M_PW:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC4_M_PW;
                    p.MeasureParams.TECParams[3].Power = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC1_M_Cur:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC1_M_Cur;
                    p.MeasureParams.TECParams[0].Curr = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC2_M_Cur:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC2_M_Cur;
                    p.MeasureParams.TECParams[1].Curr = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC3_M_Cur:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC3_M_Cur;
                    p.MeasureParams.TECParams[2].Curr = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC4_M_Cur:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC4_M_Cur;
                    p.MeasureParams.TECParams[3].Curr = data.ToSFloat;
                    break;


                case DEV_GET_CMD_TYPE.WRK_STA:
                    paramsGet = PLDParams.PLDParamsFromGet.WRK_STA;
                    p.MeasureParams.Status = (UInt32)((data.data[0]<<0)| (data.data[1] << 8)|(data.data[2] << 16) | (data.data[3] << 24));
                    break;
                case DEV_GET_CMD_TYPE.ALL_SET:
                    break;
                case DEV_GET_CMD_TYPE.ALL_M:
                    {
                        paramsGet = PLDParams.PLDParamsFromGet.ALL_M;
                        p.MeasureParams.LDParams[0].Curr= BitConverter.ToInt16(data.data, 0) / 10.0f;
                        p.MeasureParams.LDParams[1].Curr = BitConverter.ToInt16(data.data, 2) / 10.0f;
                        p.MeasureParams.LDParams[0].Vol = BitConverter.ToInt16(data.data, 4) / 10.0f;
                        p.MeasureParams.LDParams[1].Vol = BitConverter.ToInt16(data.data, 6) / 10.0f;
                        p.MeasureParams.OtherInfos.PwrTemp = BitConverter.ToInt16(data.data, 8) / 10.0f;
                        p.MeasureParams.PDParams.Power= BitConverter.ToInt16(data.data, 10) / 10.0f;
                        p.MeasureParams.PDParams.Temp = BitConverter.ToInt16(data.data, 12) / 10.0f;
                        for (int i = 0; i < 4; i++)
                        {
                            p.MeasureParams.TECParams[i].Temp = BitConverter.ToInt16(data.data, 14 + 6 * i) / 10.0f;
                            p.MeasureParams.TECParams[i].Power = BitConverter.ToInt16(data.data, 16 + 6 * i) / 10.0f;
                            p.MeasureParams.TECParams[i].Curr = BitConverter.ToInt16(data.data, 18 + 6 * i) / 10.0f;
                        }
                        p.MeasureParams.OtherInfos.SysVol = BitConverter.ToUInt16(data.data, 38) / 10.0f;
                        p.MeasureParams.OtherInfos.SysCurr = BitConverter.ToUInt16(data.data, 40) / 10.0f;
                        p.MeasureParams.Status = data.data[42]|((UInt32)data.data[43]<<8)| ((UInt32)data.data[44] << 16)| ((UInt32)data.data[45] << 24);
                        p.MeasureParams.Version = "V"+ data.data[46]+"."+ data.data[47] + "."+ data.data[48] + "."+ data.data[49];
                        p.time = data.time;
                        
                    }
                    break;
                case DEV_GET_CMD_TYPE.Upgrade:
                    paramsGet = PLDParams.PLDParamsFromGet.Upgrade;
                    p.MeasureParams.UpgradeParams.CurrIdx = BitConverter.ToUInt16(data.data);
                    p.MeasureParams.UpgradeParams.Status = (PLDParams.UpgradeStatus)data.data[2];

                    break;
                case DEV_GET_CMD_TYPE.PulseType:
                    paramsGet = PLDParams.PLDParamsFromGet.PulseType;
                    p.SetParams.PulseType= data.data[0] == (byte)PLDParams.PulseType.SPWM ? PLDParams.PulseType.SPWM : PLDParams.PulseType.NOR;
                    break;
                case DEV_GET_CMD_TYPE.LD1_Vol:
                    paramsGet = PLDParams.PLDParamsFromGet.LD1_Vol;
                    p.SetParams.LDParams[0].Vol = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD2_Vol:
                    paramsGet = PLDParams.PLDParamsFromGet.LD2_Vol;
                    p.SetParams.LDParams[1].Vol = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD1_HOC:
                    paramsGet = PLDParams.PLDParamsFromGet.LD1_HOC;
                    p.SetParams.LDParams[0].HOC = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD2_HOC:
                    paramsGet = PLDParams.PLDParamsFromGet.LD2_HOC;
                    p.SetParams.LDParams[1].HOC = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD1_SKB: paramsGet = PLDParams.PLDParamsFromGet.LD1_SKB;
                    p.SetParams.LDParams[0].CalibSet.K = BitConverter.ToInt16(data.data, 0) / 100.0f;
                    p.SetParams.LDParams[0].CalibSet.B = BitConverter.ToInt16(data.data, 2) / 100.0f;
                    break;
                case DEV_GET_CMD_TYPE.LD2_SKB: paramsGet = PLDParams.PLDParamsFromGet.LD2_SKB;
                    p.SetParams.LDParams[1].CalibSet.K = BitConverter.ToInt16(data.data, 0) / 100.0f;
                    p.SetParams.LDParams[1].CalibSet.B = BitConverter.ToInt16(data.data, 2) / 100.0f;
                    break;
                case DEV_GET_CMD_TYPE.LD1_MKB: paramsGet = PLDParams.PLDParamsFromGet.LD1_MKB;
                    p.SetParams.LDParams[0].CalibMeasure.K = BitConverter.ToInt16(data.data, 0) / 100.0f;
                    p.SetParams.LDParams[0].CalibMeasure.B = BitConverter.ToInt16(data.data, 2) / 100.0f;
                    break;
                case DEV_GET_CMD_TYPE.LD2_MKB: paramsGet = PLDParams.PLDParamsFromGet.LD2_MKB;
                    p.SetParams.LDParams[1].CalibMeasure.K = BitConverter.ToInt16(data.data, 0) / 100.0f;
                    p.SetParams.LDParams[1].CalibMeasure.B = BitConverter.ToInt16(data.data, 2) / 100.0f;
                    break;
                case DEV_GET_CMD_TYPE.PD_MKB:
                    paramsGet = PLDParams.PLDParamsFromGet.PD_MKB;
                    p.SetParams.CalibPD.K= BitConverter.ToInt16(data.data, 0) / 100.0f;
                    p.SetParams.CalibPD.B = BitConverter.ToInt16(data.data, 2) / 100.0f;
                    break;
                case DEV_GET_CMD_TYPE.TEC1_PID: paramsGet = PLDParams.PLDParamsFromGet.TEC1_PID; idx = 0; goto PID;
                case DEV_GET_CMD_TYPE.TEC2_PID: paramsGet = PLDParams.PLDParamsFromGet.TEC2_PID; idx = 1; goto PID;
                case DEV_GET_CMD_TYPE.TEC3_PID: paramsGet = PLDParams.PLDParamsFromGet.TEC3_PID; idx = 2; goto PID;
                case DEV_GET_CMD_TYPE.TEC4_PID:
                    paramsGet = PLDParams.PLDParamsFromGet.TEC4_PID; idx = 3; goto PID;
                PID:
                    {
                        p.SetParams.TECParams[idx].PID.P = BitConverter.ToInt16(data.data, 0) / 100.0f;
                        p.SetParams.TECParams[idx].PID.I = BitConverter.ToInt16(data.data, 2) / 100.0f;
                        p.SetParams.TECParams[idx].PID.D = BitConverter.ToInt16(data.data, 4) / 100.0f;
                    }
                    break;
                case DEV_GET_CMD_TYPE.S_LCM:
                    {
                        paramsGet = PLDParams.PLDParamsFromGet.S_LCM;
                        p.SetParams.LCMParams.IsPowerOn = (data.data[0] == (byte)PLDParams.WorkType.ON ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF);
                        p.SetParams.LCMParams.IsIsMotorWork = (data.data[1] == (byte)PLDParams.WorkType.ON ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF);
                        p.SetParams.LCMParams.MotorSpeed = BitConverter.ToUInt16(data.data, 2);
                        //p.SetParams.LCMParams.FanSpeed = data.data[4];
                        p.SetParams.LCMParams.FanSpeed = (data.data[4] == (byte)PLDParams.WorkType.ON ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF);
                        p.SetParams.LCMParams.PwrLimit = data.data[5];
                    }
                    break;
                case DEV_GET_CMD_TYPE.M_LCM:
                    {
                        paramsGet = PLDParams.PLDParamsFromGet.M_LCM;                       
                        p.MeasureParams.LCMParams.IsIsMotorWork = (data.data[0] == (byte)PLDParams.WorkType.ON ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF);
                        p.MeasureParams.LCMParams.MotorSpeed = BitConverter.ToUInt16(data.data, 1);
                        p.MeasureParams.LCMParams.Vol = BitConverter.ToUInt16(data.data, 3) / 10.0f;
                        p.MeasureParams.LCMParams.Curr = data.data[6] / 10.0f;
                        p.MeasureParams.LCMParams.Temp = BitConverter.ToInt16(data.data, 6) / 10.0f;
                        p.MeasureParams.LCMParams.Power = data.data[8];
                    }                    
                    break;
                case DEV_GET_CMD_TYPE.SaveParam:
                    paramsGet = PLDParams.PLDParamsFromGet.SaveParam;
                    p.MeasureParams.ParamSaveStatus = data.data[0] == 1 ? true : false;
                    break;
                case DEV_GET_CMD_TYPE.UpgradeResult:
                    paramsGet = PLDParams.PLDParamsFromGet.UpgradeResult;
                    p.MeasureParams.UpgradeResult = (PLDParams.UpgradeResult)data.data[0];
                    break;
                case DEV_GET_CMD_TYPE.BootMode:
                    paramsGet = PLDParams.PLDParamsFromGet.BootMode;

                    break;
                default:
                    break;
            }
            result = p;
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

            receiveBufferTemp.AddRange(remainData);
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

                    if (remain_cnt>= packetLen&&
                        BitConverter.ToUInt16(receiveBufferTemp.ToArray(), i + dataLen + 7) == CMD_TAIL &&
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
                        take_data_cnt = i+1;
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
                case PLDParams.PLDParamsToSet.PulseType:
                    cmd = (byte)DEV_SET_CMD_TYPE.PulseType;
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
                case PLDParams.PLDParamsToSet.Upgrade:
                    cmd = (byte)DEV_SET_CMD_TYPE.Upgrade;
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
                case PLDParams.PLDParamsToSet.SaveParam:
                    cmd = (byte)DEV_SET_CMD_TYPE.SaveParam;
                    break;
                case PLDParams.PLDParamsToSet.ClearErr:
                    cmd = (byte)DEV_SET_CMD_TYPE.ClearErr;
                    break;
                case PLDParams.PLDParamsToSet.SetVersion:
                    cmd = (byte)DEV_SET_CMD_TYPE.SetVersion;
                    break;
                default:
                    break;
            }

            frame.Add(CMD_HEAD & 0xff );
            frame.Add(CMD_HEAD >> 8);
            frame.Add(PC_ID);
            frame.Add(MCU_ID);
            frame.Add(cmd);
            frame.Add((byte)data.Length);//dataLen
            if (data.Length>0)
            {
                frame.AddRange(data);
            }
            frame.Add(SumCRC(data, data.Length));//CRC
            frame.Add(CMD_TAIL & 0xff );
            frame.Add(CMD_TAIL >> 8);

            return frame;
        }

      
     
        byte[] ConvertFloatToByte<T>(float s, int factor = 10)
        {
            float value = (float)Math.Round(s * factor);
            dynamic t = Convert.ChangeType(value, typeof(T));
            return BitConverter.GetBytes(t);
        }
        public List<byte> CreateFrame(object paramsSet, params object[] data)
        {
            List<byte> frame = new List<byte>();
            List<byte> databytes = new List<byte>();

            if (paramsSet as PLDParams.PLDParamsToSet? == null)
            {
                return frame;
            }
            byte cmd = 0;
            var a = (PLDParams.PLDParamsToSet)paramsSet;
           
            PLDParams p;
            if (data.Count()>0 && data[0] is PLDParams)
            {
                p = (PLDParams)data[0];
            }
            else
            {
                 p = new PLDParams();
            }
            
            
            switch (a)
            {
                case PLDParams.PLDParamsToSet.LD1_S_Cur:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD1_S_Cur;
                    databytes.AddRange(ConvertFloatToByte<UInt16>(p.SetParams.LDParams[0].Curr));
                    break;
                case PLDParams.PLDParamsToSet.LD2_S_Cur:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD2_S_Cur;
                    databytes.AddRange(ConvertFloatToByte<UInt16>(p.SetParams.LDParams[1].Curr));
                    break;
                case PLDParams.PLDParamsToSet.DFLT_V:
                    cmd = (byte)DEV_SET_CMD_TYPE.DFLT_V;
                    break;
                case PLDParams.PLDParamsToSet.INTER_TRG_FREQ:
                    cmd = (byte)DEV_SET_CMD_TYPE.INTER_TRG_FREQ;
                    databytes.AddRange(ConvertFloatToByte<UInt16>(p.SetParams.PulseParams.Freq,1));
                    break;
                case PLDParams.PLDParamsToSet.PULSE_WIDTH:
                    cmd = (byte)DEV_SET_CMD_TYPE.PULSE_WIDTH;
                    databytes.AddRange(ConvertFloatToByte<UInt16>(p.SetParams.PulseParams.Width, 1));
                    break;
                case PLDParams.PLDParamsToSet.Q_DELAY:
                    cmd = (byte)DEV_SET_CMD_TYPE.Q_DELAY;
                    databytes.AddRange(ConvertFloatToByte<UInt16>(p.SetParams.TQParams.Delay, 1));
                    break;
                case PLDParams.PLDParamsToSet.TRG_TYPE:
                    cmd = (byte)DEV_SET_CMD_TYPE.TRG_TYPE;
                    databytes.Add((byte)p.SetParams.TrigType);
                    break;
                case PLDParams.PLDParamsToSet.PulseType:
                    cmd = (byte)DEV_SET_CMD_TYPE.PulseType;
                    databytes.Add((byte)p.SetParams.PulseType);
                    break;
                case PLDParams.PLDParamsToSet.LD1_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD1_SW;
                    databytes.Add((byte)p.SetParams.LDParams[0].WorkType);
                    break;
                case PLDParams.PLDParamsToSet.LD2_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD2_SW;
                    databytes.Add((byte)p.SetParams.LDParams[1].WorkType);
                    break;
                case PLDParams.PLDParamsToSet.Q_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.Q_SW;
                    databytes.Add((byte)p.SetParams.TQParams.WorkType);
                    break;
                case PLDParams.PLDParamsToSet.TEC1_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC1_SW;
                    databytes.Add((byte)p.SetParams.TECParams[0].WorkType);
                    break;
                case PLDParams.PLDParamsToSet.TEC2_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC2_SW;
                    databytes.Add((byte)p.SetParams.TECParams[1].WorkType);
                    break;
                case PLDParams.PLDParamsToSet.TEC3_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC3_SW;
                    databytes.Add((byte)p.SetParams.TECParams[2].WorkType);
                    break;
                case PLDParams.PLDParamsToSet.TEC4_SW:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC4_SW;
                    databytes.Add((byte)p.SetParams.TECParams[3].WorkType);
                    break;
                case PLDParams.PLDParamsToSet.TEC1_PARA:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC1_PARA;
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[0].Temp));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[0].Vol));
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.TECParams[0].Mode));
                    break;
                case PLDParams.PLDParamsToSet.TEC2_PARA:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC2_PARA;
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[1].Temp));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[1].Vol));
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.TECParams[1].Mode));
                    break;
                case PLDParams.PLDParamsToSet.TEC3_PARA:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC3_PARA;
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[2].Temp));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[2].Vol));
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.TECParams[2].Mode));
                    break;
                case PLDParams.PLDParamsToSet.TEC4_PARA:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC4_PARA;
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[3].Temp));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[3].Vol));
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.TECParams[3].Mode));
                    break;
                case PLDParams.PLDParamsToSet.PULSE_PARA:
                    cmd = (byte)DEV_SET_CMD_TYPE.PULSE_PARA;
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.PulseParams.Num));
                    for (int i = 0; i < 11; i++)
                    {
                        databytes.AddRange(BitConverter.GetBytes(p.SetParams.PulseParams.Interval[i]));
                    }
                    break;
                case PLDParams.PLDParamsToSet.Upgrade:
                    cmd = (byte)DEV_SET_CMD_TYPE.Upgrade;
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.UpgradeParams.CurrIdx));
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.UpgradeParams.TotalPaketNum));
                    databytes.AddRange(p.SetParams.UpgradeParams.Data);
                    break;
                case PLDParams.PLDParamsToSet.LD1_Vol:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD1_Vol;
                    databytes.AddRange(ConvertFloatToByte<UInt16>(p.SetParams.LDParams[0].Vol));
                    break;
                case PLDParams.PLDParamsToSet.LD2_Vol:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD2_Vol;
                    databytes.AddRange(ConvertFloatToByte<UInt16>(p.SetParams.LDParams[1].Vol));
                    break;
                case PLDParams.PLDParamsToSet.LD1_HOC:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD1_HOC;
                    databytes.AddRange(ConvertFloatToByte<UInt16>(p.SetParams.LDParams[0].HOC));
                    break;
                case PLDParams.PLDParamsToSet.LD2_HOC:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD2_HOC;
                    databytes.AddRange(ConvertFloatToByte<UInt16>(p.SetParams.LDParams[1].HOC));
                    break;
                case PLDParams.PLDParamsToSet.LD1_SKB:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD1_SKB;
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.LDParams[0].CalibSet.K, 100));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.LDParams[0].CalibSet.B, 100));
                    break;
                case PLDParams.PLDParamsToSet.LD2_SKB:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD2_SKB;
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.LDParams[1].CalibSet.K, 100));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.LDParams[1].CalibSet.B, 100));
                    break;
                case PLDParams.PLDParamsToSet.LD1_MKB:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD1_MKB;
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.LDParams[0].CalibMeasure.K, 100));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.LDParams[0].CalibMeasure.B, 100));
                    break;
                case PLDParams.PLDParamsToSet.LD2_MKB:
                    cmd = (byte)DEV_SET_CMD_TYPE.LD2_MKB;
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.LDParams[1].CalibMeasure.K, 100));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.LDParams[1].CalibMeasure.B, 100));
                    break;
                case PLDParams.PLDParamsToSet.PD_MKB:
                    cmd = (byte)DEV_SET_CMD_TYPE.PD_MKB;
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.CalibPD.K, 100));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.CalibPD.B, 100));
                    break;
                case PLDParams.PLDParamsToSet.TEC1_PID:                    
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC1_PID;
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[0].PID.P, 100));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[0].PID.I, 100));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[0].PID.D, 100));
                    break;
                case PLDParams.PLDParamsToSet.TEC2_PID:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC2_PID;
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[1].PID.P, 100));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[1].PID.I, 100));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[1].PID.D, 100));
                    break;
                case PLDParams.PLDParamsToSet.TEC3_PID:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC3_PID;
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[2].PID.P, 100));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[2].PID.I, 100));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[2].PID.D, 100));
                    break;
                case PLDParams.PLDParamsToSet.TEC4_PID:
                    cmd = (byte)DEV_SET_CMD_TYPE.TEC4_PID;
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[3].PID.P, 100));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[3].PID.I, 100));
                    databytes.AddRange(ConvertFloatToByte<Int16>(p.SetParams.TECParams[3].PID.D, 100));
                    break;
                case PLDParams.PLDParamsToSet.LCM:
                    cmd = (byte)DEV_SET_CMD_TYPE.LCM;
                    databytes.Add((byte)p.SetParams.LCMParams.IsPowerOn);
                    databytes.Add((byte)p.SetParams.LCMParams.IsIsMotorWork);
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.LCMParams.MotorSpeed));
                    databytes.Add((byte)p.SetParams.LCMParams.FanSpeed);
                   // databytes.Add(p.SetParams.LCMParams.FanSpeed);
                    databytes.Add(p.SetParams.LCMParams.PwrLimit);
                    break;
                case PLDParams.PLDParamsToSet.SaveParam:
                    cmd = (byte)DEV_SET_CMD_TYPE.SaveParam;
                    break;
                case PLDParams.PLDParamsToSet.ClearErr:
                    cmd = (byte)DEV_SET_CMD_TYPE.ClearErr;
                    break;
                case PLDParams.PLDParamsToSet.SetVersion:
                    cmd = (byte)DEV_SET_CMD_TYPE.SetVersion;
                    break;
                default:
                    break;
            }

            frame.Add(CMD_HEAD & 0xff);
            frame.Add(CMD_HEAD >> 8);
            frame.Add(PC_ID);
            frame.Add(MCU_ID);
            frame.Add(cmd);
            frame.Add((byte)databytes.Count);//dataLen
            if (databytes.Count > 0)
            {
                frame.AddRange(databytes);
            }
            frame.Add(SumCRC(databytes.ToArray(), databytes.Count));//CRC
            frame.Add(CMD_TAIL & 0xff);
            frame.Add(CMD_TAIL >> 8);

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
                case PLDParams.PLDParamsToQuery.PulseType:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.PulseType;
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
                case PLDParams.PLDParamsToQuery.Upgrade:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.Upgrade;
                    break;
                case PLDParams.PLDParamsToQuery.UpgradeResult:
                    cmd = (byte)DEV_QUERY_CMD_TYPE.UpgradeResult;
                    break;
                    
                default:
                    break;
            }
            frame.Add(CMD_HEAD & 0xff);
            frame.Add(CMD_HEAD >> 8);
            frame.Add(PC_ID);
            frame.Add(MCU_ID);
            frame.Add(cmd);
            frame.Add(0);//dataLen
            frame.Add(0);//CRC
            frame.Add(CMD_TAIL & 0xff);
            frame.Add(CMD_TAIL >> 8);

            return frame;
        }

        private Action<object, object> _callback;
        public void SetCallback(Action<object, object> callback)
        {
            _callback = callback;
        }
    }
}
