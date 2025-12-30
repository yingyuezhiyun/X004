#ifndef __EEPROM_H__
#define __EEPROM_H__

extern uint8_t  saveWorkPara();
extern uint8_t savePara();
extern uint8_t readPara();
extern void iic_check();
extern uint8_t saveBitInfo();
#endif