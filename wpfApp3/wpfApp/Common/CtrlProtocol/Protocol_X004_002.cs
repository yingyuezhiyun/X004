using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static wpfApp.Common.CtrlProtocol.PLDParams;

namespace wpfApp.Common.CtrlProtocol
{
    public class Protocol_X004_002
    {

        public Protocol_X004_002()
        {
            // Build enum mapping dictionaries once at construction
            BuildMappings();

            Thread thread = new Thread(new ThreadStart(() => { dataHandler(); }));
            thread.Start();
        }

        // Dictionaries for fast name-based mapping between PLD enums and device enums
        private readonly Dictionary<PLDParamsToSet, DEV_SET_CMD_TYPE> setMap = new Dictionary<PLDParamsToSet, DEV_SET_CMD_TYPE>();
        private readonly Dictionary<PLDParamsToQuery, DEV_QUERY_CMD_TYPE> queryMap = new Dictionary<PLDParamsToQuery, DEV_QUERY_CMD_TYPE>();
        private readonly Dictionary<DEV_GET_CMD_TYPE, PLDParamsFromGet> getMap = new Dictionary<DEV_GET_CMD_TYPE, PLDParamsFromGet>();

        private void BuildMappings()
        {
            // Map PLDParamsToSet -> DEV_SET_CMD_TYPE by matching member names
            foreach (PLDParamsToSet s in Enum.GetValues(typeof(PLDParamsToSet)))
            {
                var name = s.ToString();
                if (Enum.TryParse<DEV_SET_CMD_TYPE>(name, out var dev))
                {
                    setMap[s] = dev;
                }
            }

            // Map PLDParamsToQuery -> DEV_QUERY_CMD_TYPE by matching member names
            foreach (PLDParamsToQuery q in Enum.GetValues(typeof(PLDParamsToQuery)))
            {
                var name = q.ToString();
                if (Enum.TryParse<DEV_QUERY_CMD_TYPE>(name, out var dev))
                {
                    queryMap[q] = dev;
                }
            }

            // Map DEV_GET_CMD_TYPE -> PLDParamsFromGet by matching member names
            foreach (DEV_GET_CMD_TYPE g in Enum.GetValues(typeof(DEV_GET_CMD_TYPE)))
            {
                var name = g.ToString();
                if (Enum.TryParse<PLDParamsFromGet>(name, out var p))
                {
                    getMap[g] = p;
                }
            }
        }

        private class PLDRev
        {
            public List<byte> rawData { get; set; } = new List<byte>();

            public byte cmd { get { return rawData[4]; } }

            public int dataLen { get { return rawData[5]; } }

            public byte[] data { get { return rawData.Skip(6).Take(dataLen).ToArray(); } }

            public float ToUFloat { get { return BitConverter.ToUInt16(data, 0) / 10.0f; } }

            public float ToSFloat { get { return BitConverter.ToInt16(data, 0) / 10.0f; } }

            public ushort ToUInt16 { get { return BitConverter.ToUInt16(data, 0); } }


            public DateTime time { get; set; }
        }

        private List<PLDRev> revdatas = new List<PLDRev>();

        private List<byte> remainData = new List<byte>();

        private object locker = new object();

        private const ushort CMD_HEAD = 0x545A;
        private const ushort CMD_TAIL = 0xFE5A;
        private const byte PC_ID = 0x14;
        private const byte MCU_ID = 0x1c;




        private enum DEV_QUERY_CMD_TYPE
        {
            PULSE_WIDTH = 0x50, /* 脉宽设定值 */
            Q_DELAY = 0x51,            /* 调Ｑ延时设定值 */
            TRG_TYPE = 0x52,           /* 触发模式 */
            LD1_S_Cur = 0x53,          /* LD电流设定值通道1 */
            TEC1_SW = 0x55,            /* TEC1开关 */
            TEC2_SW = 0x56,            /* TEC2开关 */
            TEC1_PARA = 0x57,   /* 第1路TEC：温度、限压设定值 */
            TEC2_PARA = 0x58,          /* 第2路TEC：温度、限压设定值 */
            PULSE_PARA = 0x59,  /* 子脉冲个数及脉冲间隔设定值 */
            LD1_M_Cur = 0x5A,          /* LD电流检测值通道1 */
            L1_M_V = 0x5B,      /* 负载电压检测值通道1 */
            PWR_TEMP = 0x5C,    /* 电源温度检测值 */
            OUT_PD = 0x5D,             /* 外部输入电平1（PD）检测值 */
            OUT_TEMP = 0x5E,           /* 外部输入电平2（温度）检测值 */
            TEC1_M_TEMP = 0x5F,        /* 第1路检测温度检测值 */
            TEC1_M_PW = 0x70,   /* 第1路TEC输出功率检测值 */
            TEC2_M_TEMP = 0x71,        /* 第2路检测温度检测值 */
            TEC2_M_PW = 0x72,          /* 第2路TEC输出功率检测值 */
            WRK_STA = 0x73, /* 工作状态 */
            ALL_SET = 0x74,        /* 所有设定参数 */
            ALL_M = 0x75,          /* 所有检测参数 */
            LD2_S_Cur = 0x93,   /* LD电流设定值通道2 */
            TEC3_SW = 0x95,     /* TEC3开关 */
            TEC4_SW = 0x96,            /* TEC4开关 */
            TEC3_PARA = 0xB7,   /* 第3路TEC：温度、限压、模式设定值 */
            TEC4_PARA = 0xB8,          /* 第4路TEC：温度、限压、模式设定值 */
            LD2_M_Cur = 0xBA,   /* LD电流检测值通道2 */
            L2_M_V = 0xBB,      /* 负载电压检测值通道2 */
            TEC3_M_TEMP = 0xBF, /* 第3路检测温度检测值 */
            TEC4_M_TEMP = 0xE1,        /* 第4路检测温度检测值 */
            TEC3_M_PW = 0xB0,   /* 第3路TEC输出功率检测值 */
            TEC4_M_PW = 0xE2,          /* 第4路TEC输出功率检测值 */
            BootMode = 0xE4,       /*BOOT模式*/
            LD1_SW = 0xE5,      /* LD1开关 */
            LD2_SW = 0xE6, /* LD2开关 */
            Q_SW = 0xE7,        /* Q开关 */
            PulseType = 0xE9, /* 查询脉冲模式 */
            Upgrade = 0x76,/* 查询升级程序状态 */
        }
        private enum DEV_SET_CMD_TYPE
        {

            LD1_S_Cur = 0x30,          /* LD电流设定值通道1 */
            PULSE_WIDTH = 0x33, /* 脉宽设定值 */
            Q_DELAY = 0x34,            /* 调Ｑ延时设定值 */
            TRG_TYPE = 0x35,           /* 触发模式 */
            LD1_SW = 0x36,      /* LD1开关 */
            Q_SW = 0x37,        /* Q开关 */
            TEC1_SW = 0x38,            /* TEC1开关 */
            TEC2_SW = 0x39,            /* TEC2开关 */
            TEC1_PARA = 0x3A,   /* 第1路TEC：温度、限压设定值 */
            TEC2_PARA = 0x3B,          /* 第2路TEC：温度、限压设定值 */
            PULSE_PARA = 0x3C,  /* 子脉冲个数及脉冲间隔设定值 */
            Upgrade = 0x76,/* 查询升级程序状态 */
            LD2_SW = 0xB6, /* LD2开关 */
            LD2_S_Cur = 0x90,   /* LD电流设定值通道2 */
            TEC3_SW = 0x98,     /* TEC3开关 */
            TEC4_SW = 0x99,            /* TEC4开关 */
            TEC3_PARA = 0x9A,   /* 第3路TEC：温度、限压、模式设定值 */
            TEC4_PARA = 0x9B,          /* 第4路TEC：温度、限压、模式设定值 */
            BootMode = 0xE3,       /*BOOT模式*/
            PulseType = 0xE8,/* 设置设置脉冲类型 */

        }

        private enum DEV_GET_CMD_TYPE
        {
            LD1_S_Cur = 0xA0,   /* LD电流设定值通道1 */
            PULSE_WIDTH = 0xA3,        /* 脉宽设定值 */
            Q_DELAY = 0xA4,            /* 调Ｑ延时设定值 */
            TRG_TYPE = 0xA5,           /* 触发模式 */
            LD1_SW = 0xA6,             /* LD1开关 */
            Q_SW = 0xA7,        /* Q脉冲设定值 */
            TEC1_SW = 0xA8,            /* TEC1开关 */
            TEC2_SW = 0xA9,            /* TEC2开关 */
            TEC1_PARA = 0xAA,   /* 第1路TEC：温度、限压设定值 */
            TEC2_PARA = 0xAB,          /* 第2路TEC：温度、限压设定值 */
            PULSE_PARA = 0xAC,  /* 子脉冲个数及脉冲间隔设定值 */
            LD1_M_Cur = 0xAD,          /* LD电流检测值通道1 */
            L1_M_V = 0xAE,      /* 负载电压检测值通道1 */
            PWR_TEMP = 0xAF,    /* 电源温度检测值 */
            OUT_PD = 0xC0,      /* 外部输入电平1（PD）检测值 */
            OUT_TEMP = 0xC1,           /* 外部输入电平2（温度）检测值 */
            TEC1_M_TEMP = 0xC2,        /* 第1路检测温度检测值 */
            TEC1_M_PW = 0xC3,          /* 第1路TEC输出功率检测值 */
            TEC2_M_TEMP = 0xC4,        /* 第2路检测温度检测值 */
            TEC2_M_PW = 0xC5,          /* 第2路TEC输出功率检测值 */
            WRK_STA = 0xC6,     /* 工作状态 */
            ALL_SET = 0xC7,            /* 所有设定参数 */
            ALL_M = 0xCF,       /* 所有检测参数 */
            Upgrade = 0xD0,     /* 在线程序升级 */
            LD2_SW = 0xE6,      /* LD2开关 */
            LD2_S_Cur = 0xE0,   /* LD电流设定值通道2 */
            TEC3_SW = 0xE8,     /* TEC3开关 */
            TEC4_SW = 0xE9,            /* TEC4开关 */
            TEC3_PARA = 0xEA,   /* 第3路TEC：温度、限压设定值 */
            TEC4_PARA = 0xEB,          /* 第4路TEC：温度、限压设定值 */
            LD2_M_Cur = 0xBD,   /* LD电流检测值通道2 */
            L2_M_V = 0xB9,      /* 负载电压检测值通道2 */
            TEC3_M_TEMP = 0xB2, /* 第3路检测温度检测值 */
            TEC3_M_PW = 0xB3,          /* 第3路TEC输出功率检测值 */
            TEC4_M_TEMP = 0xE4,        /* 第4路检测温度检测值 */
            TEC4_M_PW = 0xE5,          /* 第4路TEC输出功率检测值 */
            BootMode = 0xEC,/*BOOT模式*/
            PulseType = 0xE7,/* 频率触发模式 */


        }
        // Helper mapping methods: try to map between PLD enums and device enums by name.
        // Many enum member names are identical between the PLD types and device command enums,
        // so Enum.TryParse by name provides a lightweight pairing mechanism. These helpers
        // make it convenient to obtain the device command for a given PLD enum (and vice versa).
        // Removed TryMap* wrappers: use setMap/queryMap/getMap directly for lookups.
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
            PLDParamsFromGet paramsGet = PLDParamsFromGet.None;
            object result = null;
            PLDParams p = new PLDParams();
            int idx = 0;
            // Require a prebuilt mapping from device cmd -> PLDParamsFromGet.
            // Instead of using Enum.IsDefined (which can be unreliable with boxed byte values)
            // or catching exceptions, cast the incoming byte to the enum and rely on the
            // prebuilt `getMap` to determine whether we know how to handle this command.
            DEV_GET_CMD_TYPE devCmd = (DEV_GET_CMD_TYPE)data.cmd;
            if (!getMap.TryGetValue(devCmd, out var mappedGet))
            {
                return (PLDParamsFromGet.None, null);
            }
            paramsGet = mappedGet;
            switch ((DEV_GET_CMD_TYPE)data.cmd)
            {
                case DEV_GET_CMD_TYPE.LD1_S_Cur:
                    p.SetParams.LDParams[0].Curr = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD2_S_Cur:
                    p.SetParams.LDParams[1].Curr = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.PULSE_WIDTH:
                    p.SetParams.PulseParams.Width = data.ToUInt16;
                    break;
                case DEV_GET_CMD_TYPE.Q_DELAY:
                    p.SetParams.TQParams.Delay = data.ToUInt16;
                    break;
                case DEV_GET_CMD_TYPE.TRG_TYPE:
                    p.SetParams.TrigType = data.data[0] == (byte)TrigType.INTER ? TrigType.INTER : TrigType.OUT;
                    break;
                case DEV_GET_CMD_TYPE.LD1_SW:
                    p.SetParams.LDParams[0].WorkType = data.data[0] == (byte)WorkType.ON ? WorkType.ON : WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.LD2_SW:
                    p.SetParams.LDParams[1].WorkType = data.data[0] == (byte)WorkType.ON ? WorkType.ON : WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.Q_SW:
                    p.SetParams.TQParams.WorkType = data.data[0] == (byte)WorkType.ON ? WorkType.ON : WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.TEC1_SW:
                    p.SetParams.TECParams[0].WorkType = data.data[0] == (byte)WorkType.ON ? WorkType.ON : WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.TEC2_SW:
                    p.SetParams.TECParams[1].WorkType = data.data[0] == (byte)WorkType.ON ? WorkType.ON : WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.TEC3_SW:
                    p.SetParams.TECParams[2].WorkType = data.data[0] == (byte)WorkType.ON ? WorkType.ON : WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.TEC4_SW:
                    p.SetParams.TECParams[3].WorkType = data.data[0] == (byte)WorkType.ON ? WorkType.ON : WorkType.OFF;
                    break;
                case DEV_GET_CMD_TYPE.TEC1_PARA: idx = 0; goto TECParam;
                case DEV_GET_CMD_TYPE.TEC2_PARA: idx = 1; goto TECParam;
                case DEV_GET_CMD_TYPE.TEC3_PARA: idx = 2; goto TECParam;
                case DEV_GET_CMD_TYPE.TEC4_PARA:
                    idx = 3; goto TECParam;
                TECParam:
                    {
                        p.SetParams.TECParams[idx].Temp = BitConverter.ToInt16(data.data, 0) / 10.0f;
                        p.SetParams.TECParams[idx].Vol = BitConverter.ToInt16(data.data, 2) / 10.0f;
                    }
                    break;
                case DEV_GET_CMD_TYPE.PULSE_PARA:
                    {

                        p.SetParams.PulseParams.Num = BitConverter.ToUInt16(data.data, 0);
                        for (int i = 0; i < 11; i++)
                        {
                            p.SetParams.PulseParams.Interval[i] = BitConverter.ToUInt16(data.data, 2 + 2 * i);
                        }
                    }
                    break;
                case DEV_GET_CMD_TYPE.LD1_M_Cur:
                    p.MeasureParams.LDParams[0].Curr = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD2_M_Cur:
                    p.MeasureParams.LDParams[1].Curr = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.L1_M_V:
                    p.MeasureParams.LDParams[0].Vol = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.L2_M_V:
                    p.MeasureParams.LDParams[1].Vol = data.ToUFloat;
                    break;
                case DEV_GET_CMD_TYPE.PWR_TEMP:
                    p.MeasureParams.OtherInfos.PwrTemp = data.ToSFloat;

                    break;
                case DEV_GET_CMD_TYPE.OUT_PD:
                    p.MeasureParams.PDParams.Power = data.ToUFloat;

                    break;
                case DEV_GET_CMD_TYPE.OUT_TEMP:
                    p.MeasureParams.PDParams.Temp = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC1_M_TEMP:
                    p.MeasureParams.TECParams[0].Temp = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC1_M_PW:
                    p.MeasureParams.TECParams[0].Power = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC2_M_TEMP:
                    p.MeasureParams.TECParams[1].Temp = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC2_M_PW:
                    p.MeasureParams.TECParams[1].Power = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC3_M_TEMP:
                    p.MeasureParams.TECParams[2].Temp = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC3_M_PW:
                    p.MeasureParams.TECParams[2].Power = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC4_M_TEMP:
                    p.MeasureParams.TECParams[3].Temp = data.ToSFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC4_M_PW:
                    p.MeasureParams.TECParams[3].Power = data.ToSFloat;
                    break;
              
                case DEV_GET_CMD_TYPE.WRK_STA:
                    p.MeasureParams.Status = (uint)(data.data[0] << 0 | data.data[1] << 8 | data.data[2] << 16 | data.data[3] << 24);
                    break;
                case DEV_GET_CMD_TYPE.ALL_SET:
                    break;
                case DEV_GET_CMD_TYPE.ALL_M:
                    {
                        p.MeasureParams.LDParams[0].Curr = BitConverter.ToInt16(data.data, 0) / 10.0f;
                        p.MeasureParams.LDParams[1].Curr = BitConverter.ToInt16(data.data, 2) / 10.0f;
                        p.MeasureParams.LDParams[0].Vol = BitConverter.ToInt16(data.data, 4) / 10.0f;
                        p.MeasureParams.LDParams[1].Vol = BitConverter.ToInt16(data.data, 6) / 10.0f;
                        p.MeasureParams.OtherInfos.PwrTemp = BitConverter.ToInt16(data.data, 8) / 10.0f;
                        p.MeasureParams.PDParams.Power = BitConverter.ToInt16(data.data, 10) / 10.0f;
                        p.MeasureParams.PDParams.Temp = BitConverter.ToInt16(data.data, 12) / 10.0f;
                        for (int i = 0; i < 4; i++)
                        {
                            p.MeasureParams.TECParams[i].Temp = BitConverter.ToInt16(data.data, 14 + 6 * i) / 10.0f;
                            p.MeasureParams.TECParams[i].Power = BitConverter.ToInt16(data.data, 16 + 6 * i) / 10.0f;
                            p.MeasureParams.TECParams[i].Curr = BitConverter.ToInt16(data.data, 18 + 6 * i) / 10.0f;
                        }
                        p.MeasureParams.OtherInfos.SysVol = BitConverter.ToUInt16(data.data, 38) / 10.0f;
                        p.MeasureParams.OtherInfos.SysCurr = BitConverter.ToUInt16(data.data, 40) / 10.0f;
                        p.MeasureParams.Status = data.data[42] | (uint)data.data[43] << 8 | (uint)data.data[44] << 16 | (uint)data.data[45] << 24;
                        p.MeasureParams.Version = "V" + data.data[46] + "." + data.data[47] + "." + data.data[48] + "." + data.data[49];
                        p.time = data.time;

                    }
                    break;
                case DEV_GET_CMD_TYPE.Upgrade:
                    p.MeasureParams.UpgradeParams.CurrIdx = BitConverter.ToUInt16(data.data);
                    p.MeasureParams.UpgradeParams.Status = (UpgradeStatus)data.data[2];
                    break;
                case DEV_GET_CMD_TYPE.PulseType:
                    p.SetParams.PulseType = data.data[0] == (byte)PulseType.SPWM ? PulseType.SPWM : PulseType.NOR;
                    break;
               
                case DEV_GET_CMD_TYPE.BootMode:
                    p.MeasureParams.BootMode = (BootMode)data.data[0];
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

                    if (remain_cnt >= packetLen &&
                        BitConverter.ToUInt16(receiveBufferTemp.ToArray(), i + dataLen + 7) == CMD_TAIL &&
                        receiveBufferTemp[i + dataLen + 6] == SumCRC(receiveBufferTemp.Skip(i + 6).Take(dataLen).ToArray(), dataLen)
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
                        take_data_cnt = i + 1;
                    }
                }

            }
            remainData.AddRange(receiveBufferTemp.Skip(take_data_cnt));

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
            if (paramsSet as PLDParamsToSet? == null)
            {
                return frame;
            }
            byte cmd = 0;
            var a = (PLDParamsToSet)paramsSet;
            if (setMap.TryGetValue(a, out var mappedSetCmd))
            {
                cmd = (byte)mappedSetCmd;
            }
            else
            {
                // No mapping found -> cannot form a valid frame for this PLDParamsToSet
                return new List<byte>();
            }


            PLDParams p;
            if (data.Count() > 0 && data[0] is PLDParams)
            {
                p = (PLDParams)data[0];
            }
            else
            {
                p = new PLDParams();
            }


            switch (a)
            {
                case PLDParamsToSet.LD1_S_Cur:
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.LDParams[0].Curr));
                    break;
                case PLDParamsToSet.LD2_S_Cur:
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.LDParams[1].Curr));
                    break;
                case PLDParamsToSet.DFLT_V:
                    break;
                case PLDParamsToSet.INTER_TRG_FREQ:
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.PulseParams.Freq, 1));
                    break;
                case PLDParamsToSet.PULSE_WIDTH:
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.PulseParams.Width, 1));
                    break;
                case PLDParamsToSet.Q_DELAY:
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.TQParams.Delay, 1));
                    break;
                case PLDParamsToSet.TRG_TYPE:
                    databytes.Add((byte)p.SetParams.TrigType);
                    break;
                case PLDParamsToSet.PulseType:
                    databytes.Add((byte)p.SetParams.PulseType);
                    break;
                case PLDParamsToSet.LD1_SW:
                    databytes.Add((byte)p.SetParams.LDParams[0].WorkType);
                    break;
                case PLDParamsToSet.LD2_SW:
                    databytes.Add((byte)p.SetParams.LDParams[1].WorkType);
                    break;
                case PLDParamsToSet.Q_SW:
                    databytes.Add((byte)p.SetParams.TQParams.WorkType);
                    break;
                case PLDParamsToSet.TEC1_SW:
                    databytes.Add((byte)p.SetParams.TECParams[0].WorkType);
                    break;
                case PLDParamsToSet.TEC2_SW:
                    databytes.Add((byte)p.SetParams.TECParams[1].WorkType);
                    break;
                case PLDParamsToSet.TEC3_SW:
                    databytes.Add((byte)p.SetParams.TECParams[2].WorkType);
                    break;
                case PLDParamsToSet.TEC4_SW:
                    databytes.Add((byte)p.SetParams.TECParams[3].WorkType);
                    break;
                case PLDParamsToSet.TEC1_PARA:
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.TECParams[0].Temp));
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.TECParams[0].Vol));
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.TECParams[0].Mode));
                    break;
                case PLDParamsToSet.TEC2_PARA:
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.TECParams[1].Temp));
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.TECParams[1].Vol));
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.TECParams[1].Mode));
                    break;
                case PLDParamsToSet.TEC3_PARA:
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.TECParams[2].Temp));
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.TECParams[2].Vol));
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.TECParams[2].Mode));
                    break;
                case PLDParamsToSet.TEC4_PARA:
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.TECParams[3].Temp));
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.TECParams[3].Vol));
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.TECParams[3].Mode));
                    break;
                case PLDParamsToSet.PULSE_PARA:
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.PulseParams.Num));
                    for (int i = 0; i < 11; i++)
                    {
                        databytes.AddRange(BitConverter.GetBytes(p.SetParams.PulseParams.Interval[i]));
                    }
                    break;
                case PLDParamsToSet.Upgrade:
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.UpgradeParams.CurrIdx));
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.UpgradeParams.TotalPaketNum));
                    databytes.AddRange(p.SetParams.UpgradeParams.Data);
                    break;
                case PLDParamsToSet.LD1_Vol:
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.LDParams[0].Vol));
                    break;
                case PLDParamsToSet.LD2_Vol:
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.LDParams[1].Vol));
                    break;
                case PLDParamsToSet.LD1_HOC:
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.LDParams[0].HOC));
                    break;
                case PLDParamsToSet.LD2_HOC:
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.LDParams[1].HOC));
                    break;
                case PLDParamsToSet.LD1_SKB:
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.LDParams[0].CalibSet.K, 100));
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.LDParams[0].CalibSet.B, 100));
                    break;
                case PLDParamsToSet.LD2_SKB:
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.LDParams[1].CalibSet.K, 100));
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.LDParams[1].CalibSet.B, 100));
                    break;
                case PLDParamsToSet.LD1_MKB:
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.LDParams[0].CalibMeasure.K, 100));
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.LDParams[0].CalibMeasure.B, 100));
                    break;
                case PLDParamsToSet.LD2_MKB:
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.LDParams[1].CalibMeasure.K, 100));
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.LDParams[1].CalibMeasure.B, 100));
                    break;
                case PLDParamsToSet.PD_MKB:
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.CalibPD.K, 100));
                    databytes.AddRange(ConvertFloatToByte<short>(p.SetParams.CalibPD.B, 100));
                    break;
                case PLDParamsToSet.TEC1_PID:
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.TECParams[0].PID.P, 100));
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.TECParams[0].PID.I, 100));
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.TECParams[0].PID.D, 100));
                    break;
                case PLDParamsToSet.TEC2_PID:
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.TECParams[1].PID.P, 100));
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.TECParams[1].PID.I, 100));
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.TECParams[1].PID.D, 100));
                    break;
                case PLDParamsToSet.TEC3_PID:
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.TECParams[2].PID.P, 100));
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.TECParams[2].PID.I, 100));
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.TECParams[2].PID.D, 100));
                    break;
                case PLDParamsToSet.TEC4_PID:
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.TECParams[3].PID.P, 100));
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.TECParams[3].PID.I, 100));
                    databytes.AddRange(ConvertFloatToByte<ushort>(p.SetParams.TECParams[3].PID.D, 100));
                    break;
                case PLDParamsToSet.LCM:
                    databytes.Add((byte)p.SetParams.LCMParams.IsPowerOn);
                    databytes.Add((byte)p.SetParams.LCMParams.IsIsMotorWork);
                    databytes.AddRange(BitConverter.GetBytes(p.SetParams.LCMParams.MotorSpeed));
                    databytes.Add((byte)p.SetParams.LCMParams.FanSpeed);
                    // databytes.Add(p.SetParams.LCMParams.FanSpeed);
                    databytes.Add(p.SetParams.LCMParams.PwrLimit);
                    break;
                case PLDParamsToSet.SaveParam:
                    break;
                case PLDParamsToSet.ClearErr:
                    break;
                case PLDParamsToSet.SetVersion:
                    break;
                case PLDParamsToSet.BootMode:
                    databytes.Add((byte)p.SetParams.BootMode);
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
            if (paramsQuery as PLDParamsToQuery? == null)
            {
                return frame;
            }
            var a = (PLDParamsToQuery)paramsQuery;
            byte cmd = 0;
            if (queryMap.TryGetValue(a, out var mappedQueryCmd))
            {
                cmd = (byte)mappedQueryCmd;
            }
            else
            {
                // No mapping -> cannot form a valid query frame
                return new List<byte>();
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
