#include "main.h"
#include "stdint.h"
#include "string.h"
#include <stdlib.h>
#include "math.h"
#include "global_cfg.h"
#include "measure.h"
#include "max31865.h"
#include "adc_filt.h"


#define INTER_ADC_REF_V (2.5)
#define INTER_ADC_MAX_VAULE UINT16_MAX

/// @brief I*3mΩ*20+1.25V=Vadc
/// @return
float get_tec_curr(uint8_t ch)
{
    uint16_t adc = calculate_moving_average(&adc_cb[ch+12]);
    return (adc * INTER_ADC_REF_V / INTER_ADC_MAX_VAULE - 1.25) / 0.003 / 20;
}

/// @brief Vadc = Vld*2/35
/// @return
float get_ld_vol(uint8_t ch)
{
    float vol = 0;
    uint16_t adc = calculate_moving_average(&adc_cb[2 * ch + 1]);
    vol = adc * INTER_ADC_REF_V / INTER_ADC_MAX_VAULE * 35 / 2;
    // todo 是否校正
    return vol;
}

/// @brief Vadc = I * 3mΩ*50
/// @return
float get_ld_curr(uint8_t ch)
{
    float curr = 0;
    uint16_t adc = calculate_moving_average(&adc_cb[2 * ch]);
    curr = adc * INTER_ADC_REF_V / INTER_ADC_MAX_VAULE / 0.003 / 50;
    curr = curr * set_param.ld[ch].calib_measure.k + set_param.ld[ch].calib_measure.b;
    return curr;
}

float get_sys_temp(uint8_t idx)
{
    float temp = 0;
    uint16_t adc = calculate_moving_average(&adc_cb[idx + 4]);
    float V1 = (float)adc / INTER_ADC_MAX_VAULE * INTER_ADC_REF_V;
    float R1 = 51 * 1000.0 * V1 / (INTER_ADC_REF_V - V1);
    float t1 = 1177692.5 / (1203.93713 + 298.15 * log(R1)) - 273.15;
    float B1 = -0.008224 * pow((t1 + 273.15), 2) + 7.1787 * t1 + 4452.5584; // pow(x,y),求x的y次方
    temp = (B1 * 29815 / (B1 + 298.15 * log(R1) / log(exp(1)) - 2746.0629) - 27315) / 100.0;
    return temp;
}
// I*2mΩ*50=Vadc
float get_sys_curr()
{
    float curr = 0;
    uint16_t adc = calculate_moving_average(&adc_cb[10]);
    curr = adc * INTER_ADC_REF_V / INTER_ADC_MAX_VAULE * 500 / 50;
    return curr;
}
// V/28.1*2=Vadc
float get_sys_vol()
{
    float vol = 0;
    uint16_t adc = calculate_moving_average(&adc_cb[11]);
    vol = adc * INTER_ADC_REF_V / INTER_ADC_MAX_VAULE * 28.1 / 2;
    return vol;
}
float get_out_PD()
{
    float pwr = 0;
    uint16_t adc = calculate_moving_average(&adc_cb[8]);

    pwr = adc * set_param.PD_calib.k + set_param.PD_calib.b;
    return pwr;
}
float get_out_temp()
{
    float tmp = 0;
    uint16_t adc = calculate_moving_average(&adc_cb[9]);
    float V1 = (float)adc / INTER_ADC_MAX_VAULE * INTER_ADC_REF_V;
    float R1 = 10 * 1000.0 * V1 / (INTER_ADC_REF_V - V1);
    float t1 = 1177692.5 / (1203.93713 + 298.15 * log(R1)) - 273.15;
    float B1 = -0.008224 * pow((t1 + 273.15), 2) + 7.1787 * t1 + 4452.5584; // pow(x,y),求x的y次方
    tmp = (B1 * 29815 / (B1 + 298.15 * log(R1) / log(exp(1)) - 2746.0629) - 27315) / 100.0;
    return tmp;
}

void get_measure_param()
{
    for (size_t i = 0; i < 4; i++)
    {
        measure_param.tec[i].Temp = GET_TECx_Temp(i);
        measure_param.tec[i].Cur = get_tec_curr(i) - 0.65;
        measure_param.tec[i].Power = fabs(measure_param.tec[i].Cur * set_param.tec[i].OUT_V);
        measure_param.sys.Temp[i] = get_sys_temp(i);
    }
    for (size_t i = 0; i < 2; i++)
    {
        measure_param.ld[i].Cur = get_ld_curr(i) - 0.25;
        measure_param.ld[i].Vol = get_ld_vol(i) - 0.7;
    }
    measure_param.out.PD = get_out_PD();
    measure_param.out.Temp = get_out_temp();
    measure_param.sys.Cur = get_sys_curr();
    measure_param.sys.Vol = get_sys_vol();

    float max_sys_temp = 0;
    max_sys_temp = measure_param.sys.Temp[0];
    for (size_t i = 1; i < 4; i++)
    {
        if (max_sys_temp < measure_param.sys.Temp[i])
        {
            max_sys_temp = measure_param.sys.Temp[i];
        }
    }
    measure_param.PWR_Temp = max_sys_temp;
    
}
