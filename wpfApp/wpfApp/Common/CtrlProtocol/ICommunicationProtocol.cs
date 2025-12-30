using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfApp.Common.CtrlProtocol
{
    public enum DevParamsFromGet
    {
        Mode,           //工作模式
        LDTemp,         //LD温度
        LDCurr,         //LD电流
        LDPower,        //LD功率
        LD_SW,
        TEC_SW,
        LDType,         //LD类型
        LDOC,           //LD软件过流
        LDHOC,          //LD硬件过流
        LDMeasureR,     //LD测量电阻
        LDMaxCurr,      //LD最大电流

        TECMaxVol,      //TEC最大电压        
        TECMinTemp,     //TEC最小温度
        TECMaxTemp,     //TEC最大温度
        TECTempOffset,  //TEC温度偏置
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

        IsAutoRun,      //上电自动启动


        M_LDTemp,
        M_LDPower,
        M_LDCurr,
        M_PINCurr,
        M_LDVol,
        M_TECCurr,
        M_NTC,
        BoardTemp,//板载测试温度
        AHT20Temp,//AHT20温度
        AHT20Humidity,//AHT20湿度

        M_DataTime,
        M_ALL,

        Save_Para,
        REST,           //恢复出厂设置
        ClearErr,
        None,
        BIT,
        VERSION,    // 查询版本
        BUILDTIME,  // 查询版本
    }
    public enum DevParamsToSet
    {
        Mode,           //工作模式
        LDTemp,         //LD温度
        LDCurr,         //LD电流
        LDPower,        //LD功率
        LD_SW,
        TEC_SW,
        LDType,         //LD类型
        LDOC,           //LD软件过流
        LDHOC,          //LD硬件过流
        LDMeasureR,     //LD测量电阻
        LDMaxCurr,      //LD最大电流

        TECMaxVol,      //TEC最大电压        
        TECMinTemp,     //TEC最小温度
        TECMaxTemp,     //TEC最大温度
        TECTempOffset,  //TEC温度偏置
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
        Save_Para,
        REST,           //恢复出厂设置
        ClearErr,
        IsAutoRun,      //上电自动启动
    }

    public enum DevType
    {
        None,
        NotSupported,
        X002_001,
        X002_002,
    }


    public interface ICommunicationProtocol
    {
        List<byte> CreateFrame(DevParamsToSet paramsSet, params byte[] data);

        List<byte> CreateQueryFrame(DevParamsFromGet paramsSet, params byte[] data);
        (DevParamsFromGet ParamsGet, object result) ParseFrame(RevData data);

       
    }
}
