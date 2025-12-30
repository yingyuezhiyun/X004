using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfApp.ViewModels;
using static wpfApp.ViewModels.TabDevViewModel;

namespace wpfApp.Common.CtrlProtocol
{
    public class Protocol_X002_001 : ICommunicationProtocol
    {
        enum DEV_CMD_TYPE
        {
            SET_CMD = 3,//设置参数
            GET_CMD//获取参数
        }

        enum DEV_SET_CMD_TYPE
        {
            MOD = 1,
            TEMP,
            CUR,
            POWER,
            LD_SW,
            TEC_SW,
            LD_TYPE,
            OC = 0x11, // 软件过流
            HOC,       // 硬件过流
            M_R,       // 测量电阻
            TEC_MAX_V,
            MAX_CUR,//最大电流
            MIN_TEMP,//最小温度
            MAX_TMEP,//最大温度            
            PIN_PW_k,
            PIN_PW_b,
            PW_CUR_k,
            PW_CUR_b,
            TEC_k,
            TEC_b,
            CUR_S_k,
            CUR_S_b,
            CUR_M_k,
            CUR_M_b,
            LASER_p,
            LASER_i,
            LASER_d,
            TEMP_OFFSET,
            Save_Para = 0xD1,
            REST,//恢复出厂设置
            CLEAR,
            AUTO_RUN,//上电自动启动
        }

        enum DEV_GET_CMD_TYPE
        {
            MOD = 1,
            TEMP,
            CUR,
            POWER,
            LD_SW,
            TEC_SW,
            LD_TYPE,
            OC = 0x11, // 软件过流
            HOC,       // 硬件过流
            LD_R,       // 测量电阻
            TEC_MAX_V,
            MAX_CUR,//最大电流
            MIN_TEMP,//最小温度
            MAX_TMEP,//最大温度
            PIN_PW_k,
            PIN_PW_b,
            PW_CUR_k,
            PW_CUR_b,
            TEC_k,
            TEC_b,
            CUR_S_k,
            CUR_S_b,
            CUR_M_k,
            CUR_M_b,
            LASER_p,
            LASER_i,
            LASER_d,
            TEMP_OFFSET,


            M_TEMP = 0xa1,
            M_POWER,
            M_LD_CUR,
            M_PIN_CUR,
            M_LD_VOL,
            M_TEC_CUR,
            M_NTC,
            M_ALL = 0xAF,
            Save_Para = 0xD1,
            REST,//恢复出厂设置
            CLEAR,//清空错误状态
            AUTO_RUN,//上电自动启动

            BIT = 0xE1,
            ALL = 0xFF,
        }
        public List<byte> CreateFrame(DevParamsToSet paramsSet, params byte[] data)
        {
            List<byte> frame = new List<byte>();
            frame.Add((byte)DEV_CMD_TYPE.SET_CMD);
            switch (paramsSet)
            {
                case DevParamsToSet.Mode: frame.Add((byte)DEV_SET_CMD_TYPE.MOD); break;
                case DevParamsToSet.LDTemp: frame.Add((byte)DEV_SET_CMD_TYPE.TEMP); break;
                case DevParamsToSet.LDCurr: frame.Add((byte)DEV_SET_CMD_TYPE.CUR); break;
                case DevParamsToSet.LDPower: frame.Add((byte)DEV_SET_CMD_TYPE.POWER); break;
                case DevParamsToSet.LD_SW: frame.Add((byte)DEV_SET_CMD_TYPE.LD_SW); break;
                case DevParamsToSet.TEC_SW: frame.Add((byte)DEV_SET_CMD_TYPE.TEC_SW); break;
                case DevParamsToSet.LDHOC: frame.Add((byte)DEV_SET_CMD_TYPE.HOC); break;
                case DevParamsToSet.LDMeasureR: frame.Add((byte)DEV_SET_CMD_TYPE.M_R); break;
                case DevParamsToSet.LDMaxCurr: frame.Add((byte)DEV_SET_CMD_TYPE.MAX_CUR); break;
                case DevParamsToSet.TECMaxVol: frame.Add((byte)DEV_SET_CMD_TYPE.TEC_MAX_V); break;
                case DevParamsToSet.TECMaxTemp: frame.Add((byte)DEV_SET_CMD_TYPE.MAX_TMEP); break;
                case DevParamsToSet.TECMinTemp: frame.Add((byte)DEV_SET_CMD_TYPE.MIN_TEMP); break;
                case DevParamsToSet.TECTempOffset: frame.Add((byte)DEV_SET_CMD_TYPE.TEMP_OFFSET); break;
                case DevParamsToSet.Save_Para: frame.Add((byte)DEV_SET_CMD_TYPE.Save_Para); break;
                case DevParamsToSet.ClearErr: frame.Add((byte)DEV_SET_CMD_TYPE.CLEAR); break;
                case DevParamsToSet.IsAutoRun: frame.Add((byte)DEV_SET_CMD_TYPE.AUTO_RUN); break;    
                case DevParamsToSet.LDType: frame.Add((byte)DEV_SET_CMD_TYPE.LD_TYPE); break;
                case DevParamsToSet.LDOC: frame.Add((byte)DEV_SET_CMD_TYPE.OC); break;              
                case DevParamsToSet.PIN_PW_k: frame.Add((byte)DEV_SET_CMD_TYPE.PIN_PW_k); break;
                case DevParamsToSet.PIN_PW_b: frame.Add((byte)DEV_SET_CMD_TYPE.PIN_PW_b); break;
                case DevParamsToSet.PW_CUR_k: frame.Add((byte)DEV_SET_CMD_TYPE.PW_CUR_k); break;
                case DevParamsToSet.PW_CUR_b: frame.Add((byte)DEV_SET_CMD_TYPE.PW_CUR_b); break;
                case DevParamsToSet.TEC_k: frame.Add((byte)DEV_SET_CMD_TYPE.TEC_k); break;
                case DevParamsToSet.TEC_b: frame.Add((byte)DEV_SET_CMD_TYPE.TEC_b); break;
                case DevParamsToSet.CUR_S_k: frame.Add((byte)DEV_SET_CMD_TYPE.CUR_S_k); break;
                case DevParamsToSet.CUR_S_b: frame.Add((byte)DEV_SET_CMD_TYPE.CUR_S_b); break;
                case DevParamsToSet.CUR_M_k: frame.Add((byte)DEV_SET_CMD_TYPE.CUR_M_k); break;
                case DevParamsToSet.CUR_M_b: frame.Add((byte)DEV_SET_CMD_TYPE.CUR_M_b); break;
                case DevParamsToSet.LASER_p: frame.Add((byte)DEV_SET_CMD_TYPE.LASER_p); break;
                case DevParamsToSet.LASER_i: frame.Add((byte)DEV_SET_CMD_TYPE.LASER_i); break;
                case DevParamsToSet.LASER_d: frame.Add((byte)DEV_SET_CMD_TYPE.LASER_d); break;               
                case DevParamsToSet.REST: frame.Add((byte)DEV_SET_CMD_TYPE.REST); break;
              
                default: return new List<byte>();
            }

         
            frame.AddRange(data);
            frame.InsertRange(0, new List<byte> { 0x7e, 0xe7 });//帧头
            frame.Insert(2, (byte)(frame.Count + 3));//长度
            frame.AddRange(new List<byte> { 0xaa, 0x55 });//帧尾
            return frame;
        }

        public List<byte> CreateQueryFrame(DevParamsFromGet paramsSet, params byte[] data)
        {
            List<byte> frame = new List<byte>();
            frame.Add((byte)DEV_CMD_TYPE.GET_CMD);

            switch (paramsSet)
            {

                case DevParamsFromGet.Mode: frame.Add((byte)DEV_GET_CMD_TYPE.MOD); break;
                case DevParamsFromGet.LDTemp: frame.Add((byte)DEV_GET_CMD_TYPE.TEMP); break;
                case DevParamsFromGet.LDCurr: frame.Add((byte)DEV_GET_CMD_TYPE.CUR); break;
                case DevParamsFromGet.LDPower: frame.Add((byte)DEV_GET_CMD_TYPE.POWER); break;
                case DevParamsFromGet.LD_SW: frame.Add((byte)DEV_GET_CMD_TYPE.LD_SW); break;
                case DevParamsFromGet.TEC_SW: frame.Add((byte)DEV_GET_CMD_TYPE.TEC_SW); break;
                case DevParamsFromGet.LDHOC: frame.Add((byte)DEV_GET_CMD_TYPE.HOC); break;
                case DevParamsFromGet.LDMeasureR: frame.Add((byte)DEV_GET_CMD_TYPE.LD_R); break;
                case DevParamsFromGet.LDMaxCurr: frame.Add((byte)DEV_GET_CMD_TYPE.MAX_CUR); break;
                case DevParamsFromGet.TECMaxVol: frame.Add((byte)DEV_GET_CMD_TYPE.TEC_MAX_V); break;
                case DevParamsFromGet.TECMaxTemp: frame.Add((byte)DEV_GET_CMD_TYPE.MAX_TMEP); break;
                case DevParamsFromGet.TECMinTemp: frame.Add((byte)DEV_GET_CMD_TYPE.MIN_TEMP); break;
                case DevParamsFromGet.TECTempOffset: frame.Add((byte)DEV_GET_CMD_TYPE.TEMP_OFFSET); break;
                case DevParamsFromGet.Save_Para: frame.Add((byte)DEV_GET_CMD_TYPE.Save_Para); break;
                case DevParamsFromGet.ClearErr: frame.Add((byte)DEV_GET_CMD_TYPE.CLEAR); break;
                case DevParamsFromGet.IsAutoRun: frame.Add((byte)DEV_GET_CMD_TYPE.AUTO_RUN); break;

                case DevParamsFromGet.LDType: frame.Add((byte)DEV_GET_CMD_TYPE.LD_TYPE); break;
                case DevParamsFromGet.LDOC: frame.Add((byte)DEV_GET_CMD_TYPE.OC); break;
                case DevParamsFromGet.PIN_PW_k: frame.Add((byte)DEV_GET_CMD_TYPE.PIN_PW_k); break;
                case DevParamsFromGet.PIN_PW_b: frame.Add((byte)DEV_GET_CMD_TYPE.PIN_PW_b); break;
                case DevParamsFromGet.PW_CUR_k: frame.Add((byte)DEV_GET_CMD_TYPE.PW_CUR_k); break;
                case DevParamsFromGet.PW_CUR_b: frame.Add((byte)DEV_GET_CMD_TYPE.PW_CUR_b); break;
                case DevParamsFromGet.TEC_k: frame.Add((byte)DEV_GET_CMD_TYPE.TEC_k); break;
                case DevParamsFromGet.TEC_b: frame.Add((byte)DEV_GET_CMD_TYPE.TEC_b); break;
                case DevParamsFromGet.CUR_S_k: frame.Add((byte)DEV_GET_CMD_TYPE.CUR_S_k); break;
                case DevParamsFromGet.CUR_S_b: frame.Add((byte)DEV_GET_CMD_TYPE.CUR_S_b); break;
                case DevParamsFromGet.CUR_M_k: frame.Add((byte)DEV_GET_CMD_TYPE.CUR_M_k); break;
                case DevParamsFromGet.CUR_M_b: frame.Add((byte)DEV_GET_CMD_TYPE.CUR_M_b); break;
                case DevParamsFromGet.LASER_p: frame.Add((byte)DEV_GET_CMD_TYPE.LASER_p); break;
                case DevParamsFromGet.LASER_i: frame.Add((byte)DEV_GET_CMD_TYPE.LASER_i); break;
                case DevParamsFromGet.LASER_d: frame.Add((byte)DEV_GET_CMD_TYPE.LASER_d); break;
                case DevParamsFromGet.M_LDTemp: frame.Add((byte)DEV_GET_CMD_TYPE.M_TEMP); break;
                case DevParamsFromGet.M_LDPower: frame.Add((byte)DEV_GET_CMD_TYPE.M_POWER); break;
                case DevParamsFromGet.M_LDCurr: frame.Add((byte)DEV_GET_CMD_TYPE.M_LD_CUR); break;
                case DevParamsFromGet.M_PINCurr: frame.Add((byte)DEV_GET_CMD_TYPE.M_PIN_CUR); break;
                case DevParamsFromGet.M_LDVol: frame.Add((byte)DEV_GET_CMD_TYPE.M_LD_VOL); break;
                case DevParamsFromGet.M_TECCurr: frame.Add((byte)DEV_GET_CMD_TYPE.M_TEC_CUR); break;
                case DevParamsFromGet.M_NTC: frame.Add((byte)DEV_GET_CMD_TYPE.M_NTC); break;
                case DevParamsFromGet.M_ALL: frame.Add((byte)DEV_GET_CMD_TYPE.M_ALL); break;
                case DevParamsFromGet.REST: frame.Add((byte)DEV_GET_CMD_TYPE.REST); break;
                case DevParamsFromGet.BIT: frame.Add((byte)DEV_GET_CMD_TYPE.BIT); break;
                default: return new List<byte>();
            }


            frame.AddRange(data);
            frame.InsertRange(0, new List<byte> { 0x7e, 0xe7 });//帧头
            frame.Insert(2, (byte)(frame.Count + 3));//长度
            frame.AddRange(new List<byte> { 0xaa, 0x55 });//帧尾
            return frame;
        }

        public class type_map_t
        {
            public string Name;
            public byte Mode;
        }
        type_map_t[] work_modes =
        {
            new type_map_t() { Name = "恒电流",    Mode = 0 },
            new type_map_t() { Name = "恒功率",    Mode = 1 },
            new type_map_t() { Name = "外调制",    Mode = 2 },
        };
        string WorkModeToString(byte mode)
        {
            foreach (var item in work_modes)
            {
                if (item.Mode == mode)
                {
                    return item.Name;
                }
            }
            return "其他";
        }

        string LDStatusToString(byte mode)
        {

            switch (mode)
            {
                case 0: return "待机";
                case 1: return "运行中";
                case 2: return "过流";
                case 3: return "欠流";
                default: return "待机";
            }
        }

        string TECStatusToString(byte mode)
        {
            switch (mode)
            {
                case 0: return "待机";
                case 1: return "运行中";
                case 2: return "过温";
                case 3: return "欠温";
                default: return "待机";
            }
        }
      
        

        public (DevParamsFromGet ParamsGet, object result) ParseFrame(RevData data)
        {
            DevParamsFromGet paramsGet = DevParamsFromGet.None;
            object result = null;
            if (data.cmdType != (byte)DEV_CMD_TYPE.GET_CMD)
            {
                return (paramsGet, result);
            }

            switch ((DEV_GET_CMD_TYPE)data.cmd)
            {
                case DEV_GET_CMD_TYPE.MOD:
                    paramsGet = DevParamsFromGet.Mode;
                    result = WorkModeToString(data.data[0]);
                    break;
                case DEV_GET_CMD_TYPE.TEMP:
                    paramsGet = DevParamsFromGet.LDTemp;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.CUR:
                    paramsGet = DevParamsFromGet.LDCurr;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.POWER:
                    paramsGet = DevParamsFromGet.LDPower;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD_SW:
                    paramsGet = DevParamsFromGet.LD_SW;
                    result = LDStatusToString(data.data[0]);
                    break;
                case DEV_GET_CMD_TYPE.TEC_SW:
                    paramsGet = DevParamsFromGet.TEC_SW;
                    result = TECStatusToString(data.data[0]);
                    break;
                case DEV_GET_CMD_TYPE.LD_TYPE:
                    break;
                case DEV_GET_CMD_TYPE.OC:
                    break;
                case DEV_GET_CMD_TYPE.HOC:
                    paramsGet = DevParamsFromGet.LDHOC;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.LD_R:
                    paramsGet = DevParamsFromGet.LDMeasureR;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC_MAX_V:
                    paramsGet = DevParamsFromGet.TECMaxVol;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.MAX_CUR:
                    paramsGet = DevParamsFromGet.LDMaxCurr;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.MIN_TEMP:
                    paramsGet = DevParamsFromGet.TECMinTemp;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.MAX_TMEP:
                    paramsGet = DevParamsFromGet.TECMaxTemp;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.PIN_PW_k:
                    paramsGet = DevParamsFromGet.PIN_PW_k;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.PIN_PW_b:
                    paramsGet = DevParamsFromGet.PIN_PW_b;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.PW_CUR_k:
                    paramsGet = DevParamsFromGet.PW_CUR_k;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.PW_CUR_b:
                    paramsGet = DevParamsFromGet.PW_CUR_b;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC_k:
                    paramsGet = DevParamsFromGet.TEC_k;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEC_b:
                    paramsGet = DevParamsFromGet.TEC_b;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.CUR_S_k:
                    paramsGet = DevParamsFromGet.CUR_S_k;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.CUR_S_b:
                    paramsGet = DevParamsFromGet.CUR_S_b;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.CUR_M_k:
                    paramsGet = DevParamsFromGet.CUR_M_k;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.CUR_M_b:
                    paramsGet = DevParamsFromGet.CUR_M_b;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.LASER_p:
                    paramsGet = DevParamsFromGet.LASER_p;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.LASER_i:
                    paramsGet = DevParamsFromGet.LASER_i;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.LASER_d:
                    paramsGet = DevParamsFromGet.LASER_d;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.TEMP_OFFSET:
                    paramsGet = DevParamsFromGet.TECTempOffset;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.M_TEMP:
                    paramsGet = DevParamsFromGet.M_LDPower;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.M_POWER:
                    paramsGet = DevParamsFromGet.M_LDPower;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.M_LD_CUR:
                    paramsGet = DevParamsFromGet.M_LDCurr;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.M_PIN_CUR:
                    paramsGet = DevParamsFromGet.M_PINCurr;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.M_LD_VOL:
                    paramsGet = DevParamsFromGet.M_LDVol;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.M_TEC_CUR:
                    paramsGet = DevParamsFromGet.M_TECCurr;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.M_NTC:
                    paramsGet = DevParamsFromGet.M_NTC;
                    result = data.ToFloat;
                    break;
                case DEV_GET_CMD_TYPE.M_ALL:
                    //todo
                    if (data.dataLen >= 7 * sizeof(float))
                    {
                        paramsGet = DevParamsFromGet.M_ALL;
                        Dictionary<DevParamsFromGet, object> pairs = new Dictionary<DevParamsFromGet, object>();
                        pairs[DevParamsFromGet.M_DataTime]= data.time;
                        pairs[DevParamsFromGet.M_LDTemp] = BitConverter.ToSingle(data.data, 0);
                        pairs[DevParamsFromGet.M_LDPower] = BitConverter.ToSingle(data.data, 4);
                        pairs[DevParamsFromGet.M_LDCurr] = BitConverter.ToSingle(data.data, 8);
                        pairs[DevParamsFromGet.M_PINCurr] = BitConverter.ToSingle(data.data, 12);
                        pairs[DevParamsFromGet.M_LDVol] = BitConverter.ToSingle(data.data, 16);
                        pairs[DevParamsFromGet.M_TECCurr] = BitConverter.ToSingle(data.data, 20);
                        pairs[DevParamsFromGet.M_NTC] = BitConverter.ToSingle(data.data, 24);
                        result = pairs;
                    }

                    break;
                case DEV_GET_CMD_TYPE.Save_Para:
                    paramsGet = DevParamsFromGet.Save_Para;
                    result = data.data[0] == (byte)1 ? "固化成功!" : "固化失败！";
                    break;
                case DEV_GET_CMD_TYPE.REST:
                    break;
                case DEV_GET_CMD_TYPE.CLEAR:
                    break;
                case DEV_GET_CMD_TYPE.AUTO_RUN:
                    paramsGet = DevParamsFromGet.IsAutoRun;
                    result = data.data[0] == (byte)1 ? "是" : "否";
                    break;
                case DEV_GET_CMD_TYPE.BIT:
                    paramsGet = DevParamsFromGet.BIT;
                    result = data.ToUInt32; 
                    break;
                case DEV_GET_CMD_TYPE.ALL:
                    break;
                default:
                    break;
            }
            return (paramsGet, result);

        }

       
    }
}
