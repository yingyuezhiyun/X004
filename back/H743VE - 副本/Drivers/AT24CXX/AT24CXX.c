#include "AT24CXX.h"
#include "i2c.h"
#include "stdbool.h"
#include "cmsis_os.h"
#include "global_cfg.h"


// 初始化IIC接口
/* Exported constants (常量) --------------------------------------------------------*/
#define AT24CXX_HANDLE &hi2c1
/* Exported macro (宏) --------------------------------------------------------------*/

#define AT24C_DEV_ADDR 0XA0 /* 设备地址 */
/* Exported variables (变量)---------------------------------------------------------*/

/* Exported functions (函数) ------------------------------------------------------- */

// 1us 延时函数
void I2C_delay(uint16_t nus)
{
    uint16_t i;
    while (nus--)
    {
        for (i = 0; i < 220; i++)
            ;
    }
}

#if USE_HARDWARE_IIC == 0

#define I2C_SCL_Pin GPIO_PIN_10
#define I2C_SDA_Pin GPIO_PIN_11
#define I2C_GPIO GPIOB
#define SCL_H (I2C_GPIO->BSRR = I2C_SCL_Pin)
#define SCL_L (I2C_GPIO->BRR = I2C_SCL_Pin)
#define SDA_H (I2C_GPIO->BSRR = I2C_SDA_Pin)
#define SDA_L (I2C_GPIO->BRR = I2C_SDA_Pin)
#define SCL_read (I2C_GPIO->IDR & I2C_SCL_Pin)
#define SDA_read (I2C_GPIO->IDR & I2C_SDA_Pin)

bool iic_start(void)
{
    SDA_H;
    SCL_H;
    I2C_delay(4);
    if (!SDA_read)
        return false; // SDA线为低电平则总线忙,退出
    SDA_L;
    I2C_delay(1);
    if (SDA_read)
        return false; // SDA线为高电平则总线出错,退出
    SDA_L;
    I2C_delay(4);
    SCL_L; // 钳住总线，准备发送或者接收数据
    SDA_H;
    return true;
}

void iic_stop(void)
{
    SCL_L;
    I2C_delay(1);
    SDA_L;
    I2C_delay(4);
    SCL_H;
    I2C_delay(1);
    SDA_H;
    I2C_delay(1);
    SCL_L;
}

void I2C_Ack(void)
{
    SCL_L;
    I2C_delay(1);
    SDA_L;
    I2C_delay(1);
    SCL_H;
    I2C_delay(1);
    SCL_L;
    I2C_delay(1);
}

void I2C_NoAck(void)
{
    SCL_L;
    I2C_delay(1);
    SDA_H;
    I2C_delay(1);
    SCL_H;
    I2C_delay(1);
    SCL_L;
    I2C_delay(1);
}

bool iic_wait_ack(void) // 返回为:=1有ACK,=0无ACK
{
    uint8_t i;

    SCL_L;
    I2C_delay(1);
    SDA_H;
    I2C_delay(1);
    SCL_H;
    I2C_delay(1);

    for (i = 0; i < 120; i++)
    {
        if (SDA_read == 0) //============================= ?
        {
            SCL_L;
            return true;
        }
    }
    SCL_L;
    return false;
}

uint8_t iic_send_byte(uint8_t SendByte) // 数据从高位到低位//
{
    uint8_t i = 8;
    while (i--)
    {
        SCL_L;
        I2C_delay(1);
        if (SendByte & 0x80)
            SDA_H;
        else
            SDA_L;
        SendByte <<= 1;
        I2C_delay(1);
        SCL_H;
        I2C_delay(1);
    }
    SCL_L;

    if (iic_wait_ack()) // 等待应答
        return true;
    else
        return false;
}

uint8_t iic_read_byte(bool last_char) // 数据从高位到低位//
{
    uint8_t i = 8;
    uint8_t ReceiveByte = 0;

    SDA_H;
    while (i--)
    {
        ReceiveByte <<= 1;
        SCL_L;
        I2C_delay(1);
        SCL_H;
        I2C_delay(1);
        if (SDA_read)
            ReceiveByte |= 0x01;
        else
            I2C_delay(1);
    }
    SCL_L;

    if (last_char) // 最后一个字符不需要应答信号
    {
        I2C_NoAck();
    }
    else
    { // 需要应答
        I2C_Ack();
    }

    return ReceiveByte;
}

#endif


/*****************************************
函数名：void AT24CXX_WriteOneByte(uint16_t WriteAddr,uint8_t DataToWrite)
参数：WriteAddr :要写入数据的地址  DataToWrite：要写入的数据
功能描述：从指定地址开始写入1个字节数据
返回值：无
*****************************************/
void AT24CXX_WriteOneByte(uint16_t WriteAddr, uint8_t *DataToWrite)
{
#if USE_HARDWARE_IIC == 1
    uint8_t buf[2] = {WriteAddr >> 8, WriteAddr % 256};

    if (EE_TYPE > AT24C16)
    {
        if (HAL_I2C_Mem_Write(AT24CXX_HANDLE, AT24C_DEV_ADDR, WriteAddr, I2C_MEMADD_SIZE_8BIT, DataToWrite, 1, HAL_MAX_DELAY) != HAL_OK)
        {
        }
    }
    else if (HAL_I2C_Mem_Write(AT24CXX_HANDLE, AT24C_DEV_ADDR | ((*buf & 0x07) << 1), *(buf + 1), I2C_MEMADD_SIZE_8BIT, DataToWrite, 1, HAL_MAX_DELAY) != HAL_OK)
    {
    }

#else
    /* 根据不同的24CXX型号，发送高位地址
     * 1，24C16以上的型号，分2个字节发送地址
     * 2，24C16及以下的型号，发送1个低字节地址 + 占用器件地址的bit1~bit3位（用于表示高位地址，最多11位地址）
     *    对于24C01/02，其器件地址格式（8bit）为: 1 0 1 0 A2  A1 A0 R/W
     *    对于24C04，   其器件地址格式（8bit）为: 1 0 1 0 A2  A1 a8 R/W
     *    对于24C08，   其器件地址格式（8bit）为: 1 0 1 0 A2  a9 a8 R/W
     *    对于24C16，   其器件地址格式（8bit）为: 1 0 1 0 a10 a9 a8 R/W
     *    R/W      : 读/写控制位，0：写；1：读
     *    A0/A1/A2 : 对应器件的1/2/3引脚（只有24C01/02/04/08有这些脚）
     *    a8/a9/a10: 对应存储整列的高位地址，11bit地址最多可以表示2048个位置，可以寻址24C16及以内的型号
     */
    iic_start();           /* 产生IIC起始信号 */
    if (EE_TYPE > AT24C16) /* 24C16以上的型号，分2个字节发送地址 */
    {
        iic_send_byte(AT24C_DEV_ADDR); /* 发送写命令 */
        iic_wait_ack();                /* 每发送完一个字节都要等待ACK */
        iic_send_byte(WriteAddr >> 8); /* 发送高字节地址 */
    }
    else /* 24C16及以下的型号，发送1个低字节地址 + 占用器件地址的bit1~bit3位（用于表示高位地址，最多11位地址） */
    {
        iic_send_byte(AT24C_DEV_ADDR + ((WriteAddr >> 8) << 1)); /* 发送0xA0+高位a8/a9/a10地址，写命令 */
    }
    iic_wait_ack();                 /* 每发送完一个字节都要等待ACK */
    iic_send_byte(WriteAddr % 256); /* 发送低位地址 */
    iic_wait_ack();

    iic_send_byte(WriteAddr); /* IIC发送一个字节 */
    iic_wait_ack();
    iic_stop();    /* 产生IIC停止信号 */
    HAL_Delay(10); /* EEPROM的写入比较慢，必须等到10ms后再写下一个字节 */
#endif
}

/*****************************************
函数名：AT24CXX_WritePage(uint16_t WriteAddr,uint8_t u8Len, uint8_t *pData)
参数： WriteAddr : 要写入数据的地址
            u8Len：要写入的数据
            pData：要写入的数据的首地址
功能描述：从指定地址开始写入1个字节数据
返回值：无
*****************************************/
void AT24CXX_WritePage(uint16_t WriteAddr, uint8_t u8Len, uint8_t *pData)
{
#if USE_HARDWARE_IIC == 1
    uint8_t buf[2] = {WriteAddr >> 8, WriteAddr % 256};

    if (u8Len > PAGE_SIZE) // 长度大于页的长度
    {
        u8Len = PAGE_SIZE;
    }
    if ((WriteAddr + (uint16_t)u8Len) > CAPACITY_SIZE) // 超过容量
    {
        u8Len = (uint8_t)(CAPACITY_SIZE - WriteAddr);
    }
    if (((WriteAddr % PAGE_SIZE) + (uint16_t)u8Len) > PAGE_SIZE) // 判断是否跨页
    {
        u8Len -= (uint8_t)((WriteAddr + (uint16_t)u8Len) % PAGE_SIZE); // 跨页，截掉跨页的部分
    }

    if (EE_TYPE > AT24C16)
    {
        if (HAL_I2C_Mem_Write(AT24CXX_HANDLE, AT24C_DEV_ADDR, WriteAddr, I2C_MEMADD_SIZE_8BIT, pData, u8Len, HAL_MAX_DELAY) != HAL_OK)
        {
        }
    }
    else
    {
        if (EE_TYPE <= AT24C02)
        {
            HAL_I2C_Mem_Write(AT24CXX_HANDLE, AT24C_DEV_ADDR, *(buf + 1), I2C_MEMADD_SIZE_8BIT, pData, u8Len, HAL_MAX_DELAY);
        }
        else if (HAL_I2C_Mem_Write(AT24CXX_HANDLE, AT24C_DEV_ADDR | ((*buf & 0x07) << 1), *(buf + 1), I2C_MEMADD_SIZE_8BIT, pData, u8Len, HAL_MAX_DELAY) != HAL_OK)
        {
        }
    }
#endif
}
/*****************************************
函数名：uint8_t AT24CXX_ReadOneByte(uint16_t ReadAddr)
参数： ReadAddr：要读取数据的地址 pBuffer：回填数据首地址
功能描述：从指定地址开始读取1个字节数据
返回值：返回读取到的数据
*****************************************/
uint8_t AT24CXX_ReadOneByte(uint16_t ReadAddr)
{
#if USE_HARDWARE_IIC == 1
    uint8_t buf[2] = {ReadAddr >> 8, ReadAddr % 256}, DataToRead = 0;
    if (EE_TYPE > AT24C16)
    {
        if (HAL_I2C_Master_Transmit(AT24CXX_HANDLE, AT24C_DEV_ADDR, buf, 2, 0xff) != HAL_OK)
        {
        }
    }
    else
    {
        if (HAL_I2C_Master_Transmit(AT24CXX_HANDLE, AT24C_DEV_ADDR | ((*buf & 0x07) << 1), buf + 1, 1, 0xff) != HAL_OK)
        {
        }
    }
    if (HAL_I2C_Master_Receive(AT24CXX_HANDLE, AT24C_DEV_ADDR | ((*buf & 0x07) << 1), &DataToRead, 1, 0xff) != HAL_OK)
    {
    }
    return DataToRead;
#else
    uint8_t data;

    /* 根据不同的24CXX型号，发送高位地址
     * 1，24C16以上的型号，分2个字节发送地址
     * 2，24C16及以下的型号，发送1个低字节地址 + 占用器件地址的bit1~bit3位（用于表示高位地址，最多11位地址）
     *    对于24C01/02，其器件地址格式（8bit）为: 1 0 1 0 A2  A1 A0 R/W
     *    对于24C04，   其器件地址格式（8bit）为: 1 0 1 0 A2  A1 a8 R/W
     *    对于24C08，   其器件地址格式（8bit）为: 1 0 1 0 A2  a9 a8 R/W
     *    对于24C16，   其器件地址格式（8bit）为: 1 0 1 0 a10 a9 a8 R/W
     *    R/W      : 读/写控制位，0：写；1：读
     *    A0/A1/A2 : 对应器件的1/2/3引脚（只有24C01/02/04/08有这些脚）
     *    a8/a9/a10: 对应存储整列的高位地址，11bit地址最多可以表示2048个位置，可以寻址24C16及以内的型号
     */
    iic_start();           /* 产生IIC起始信号 */
    if (EE_TYPE > AT24C16) /* 24C16以上的型号，分2个字节发送地址 */
    {
        iic_send_byte(AT24C_DEV_ADDR); /* 发送写命令 */
        iic_wait_ack();                /* 每发送完一个字节都要等待ACK */
        iic_send_byte(ReadAddr >> 8);  /* 发送高字节地址 */
    }
    else /* 24C16及以下的型号，发送1个低字节地址 + 占用器件地址的bit1~bit3位（用于表示高位地址，最多11位地址） */
    {
        iic_send_byte(AT24C_DEV_ADDR + ((ReadAddr >> 8) << 1)); /* 发送0xA0+高位a8/a9/a10地址，写命令 */
    }
    iic_wait_ack();                /* 每发送完一个字节都要等待ACK */
    iic_send_byte(ReadAddr % 256); /* 发送低位地址 */
    iic_wait_ack();

    iic_start();                       /* 产生IIC起始信号 */
    iic_send_byte(AT24C_DEV_ADDR + 1); /* 发送读命令 */
    iic_wait_ack();
    data = iic_read_byte(1); /* IIC读取一个字节 */
    iic_stop();              /* 产生IIC停止信号 */

    return data;
#endif
}

/*****************************************
函数名：uint8_t AT24CXX_ReadOneByte(uint16_t ReadAddr)
参数： ReadAddr：要读取数据的地址 pBuffer：回填数据首地址
参数： WriteAddr : 读取的首地址
            u8Len：读取数据字节数，最大为CAPACITY_SIZE
            pBuff：读取数据存入的缓存
功能描述：从指定地址开始读取1个字节数据
返回值：返回读取到的数据
*****************************************/
void x24Cxx_ReadNByte(uint16_t ReadAddr, uint8_t *pBuff, uint16_t u16Len)
{
    
    
#if USE_HARDWARE_IIC == 1
    uint8_t buf[2] = {ReadAddr >> 8, ReadAddr % 256};
    if (EE_TYPE <= AT24C16)
    { // 器件寻址+写+页选择位的处理
        if (EE_TYPE > AT24C02)
            buf[0] = AT24C_DEV_ADDR | ((*buf & 0x07) << 1);
        else
            buf[0] = AT24C_DEV_ADDR;
    }

    if (u16Len > CAPACITY_SIZE) // 读取长度大于存储器的总容量(字节)
        u16Len = CAPACITY_SIZE;
    if ((ReadAddr + (uint16_t)u16Len) > CAPACITY_SIZE) // 超过容量，截掉超过的部分
        u16Len = (uint8_t)(CAPACITY_SIZE - ReadAddr);

    if (EE_TYPE > AT24C16)
    {
        if (HAL_I2C_Master_Transmit(AT24CXX_HANDLE, AT24C_DEV_ADDR, buf, 2 * u16Len, 0xff) != HAL_OK)
        {
        }
    }
    else
    {
        if (HAL_I2C_Master_Transmit(AT24CXX_HANDLE, buf[0], buf + 1, 1, 0xff) != HAL_OK)
        {
        }
    }
    if (HAL_I2C_Master_Receive(AT24CXX_HANDLE, buf[0], pBuff, u16Len, 0xff) != HAL_OK)
    {
    }
#else
    for (size_t i = 0; i < u16Len; i++)
    {
        pBuff[i] = AT24CXX_ReadOneByte(ReadAddr + i);
    }

#endif
}

/*****************************************
函数名：void AT24CXX_Write(uint16_t WriteAddr,uint8_t *pBuffer,uint16_t NumToWrite)
参数：WriteAddr :要写入数据的地址  pBuffer：要写入的数据的首地址 NumToWrite：要写入数据的长度
功能描述：从指定地址开始写入多个字节数据
返回值：无
*****************************************/
void AT24CXX_Write(uint16_t WriteAddr, uint8_t *pBuffer, uint16_t NumToWrite)
{
    uint8_t Fill_PAGE_SIZ, Number_PAGE_SIZ, Broken_PAGE_SIZ;
    Fill_PAGE_SIZ = PAGE_SIZE - (WriteAddr % PAGE_SIZE); // 从写入地址开始填满一页所需的字节
    if (NumToWrite > Fill_PAGE_SIZ)
        Number_PAGE_SIZ = (NumToWrite - Fill_PAGE_SIZ) >> 4; // 需要填写的页数
    else
        Number_PAGE_SIZ = 0;
    if (NumToWrite > Fill_PAGE_SIZ) // 最后不够一页的字节
        Broken_PAGE_SIZ = NumToWrite - Fill_PAGE_SIZ - Number_PAGE_SIZ * PAGE_SIZE;
    else
        Broken_PAGE_SIZ = 0;

    if (NumToWrite > 1)
    {
        if (NumToWrite > CAPACITY_SIZE)
            NumToWrite = CAPACITY_SIZE;
        if ((WriteAddr + (uint16_t)NumToWrite) > CAPACITY_SIZE) // 超过容量
        {
            NumToWrite = (uint8_t)(CAPACITY_SIZE - WriteAddr);
        }
        if (NumToWrite > Fill_PAGE_SIZ)
        {
            AT24CXX_WritePage(WriteAddr, Fill_PAGE_SIZ, pBuffer); // 从写的起始地址中写满一页
            WriteAddr += Fill_PAGE_SIZ;
            pBuffer += Fill_PAGE_SIZ;
            Delay_ms(5);
            for (uint8_t i = 0; i < Number_PAGE_SIZ; i++) // 写完剩下的页数
            {
                AT24CXX_WritePage(WriteAddr, PAGE_SIZE, pBuffer);
                WriteAddr += PAGE_SIZE;
                pBuffer += PAGE_SIZE;
                Delay_ms(5);
            }
            Delay_ms(5);
            if (Broken_PAGE_SIZ != 0)
                AT24CXX_WritePage(WriteAddr, Broken_PAGE_SIZ, pBuffer); // 写完剩下的不够一页的字节
        }
        else
        {
            AT24CXX_WritePage(WriteAddr, NumToWrite, pBuffer); // 写完不够一页的字节
        }

        //        AT24CXX_WritePage(WriteAddr,NumToWrite,pBuffer);
    }
    else
    {
        AT24CXX_WriteOneByte(WriteAddr, pBuffer);
        Delay_ms(5);
    }
}

void AT24CXX_Write2(uint16_t WriteAddr, uint8_t *pBuffer, uint16_t NumToWrite)
{
    for (size_t i = 0; i < NumToWrite; i++)
    {
        AT24CXX_WriteOneByte(WriteAddr+i, (pBuffer+i));
        Delay_ms(5);
    }
}

/*****************************************
函数名：AT24CXX_Read(uint16_t ReadAddr,uint8_t *pBuffer,uint16_t NumToRead)
参数： ReadAddr：要读取数据的地址 pBuffer：回填数据首地址 NumToRead:数据长度
功能描述：从指定地址开始读取多个字节数据
返回值：无
*****************************************/
void AT24CXX_Read(uint16_t ReadAddr, uint8_t *pBuffer, uint16_t NumToRead)
{
    if (NumToRead > 1)
    {
        x24Cxx_ReadNByte(ReadAddr, pBuffer, NumToRead);
    }
    else
    {
        pBuffer[0] = AT24CXX_ReadOneByte(ReadAddr);
    }
}
/*****************************************
函数名：uint8_t AT24CXX_Check(void)
参数：无
功能描述：检查AT24CXX是否正常，这里用了24XX的最后一个地址(255)来存储标志字.如果用其他24C系列,这个地址要修改
返回值：
*****************************************/
uint8_t AT24CXX_Check(void)
{
    uint8_t temp;
    uint8_t data = IIC_CHECK_BYTE;
    AT24CXX_Read(EE_TYPE, &temp, 1); // 避免每次开机都写AT24CXX
    if (temp == IIC_CHECK_BYTE)
        return IIC_Check_OK;
    Delay_ms(10);
    AT24CXX_Write(EE_TYPE, &data, 1);
    Delay_ms(10);
    AT24CXX_Read(EE_TYPE, &temp, 1);
    if (temp != IIC_CHECK_BYTE)
        return IIC_Check_Fail;
    return IIC_Check_OK_With_Empty;
}
/*****************************************
函数名：void AT24CXX_PAGE_Check(void)
参数：     无
功能描述： 检查AT24CXX在页与页之间随机的任意地址写入任意长度的数据（数据长度不能超过存储容量）
返回值：    无
*****************************************/
void AT24CXX_PAGE_Check(void)
{
    uint8_t judge = 0XA5;
    uint8_t temp[40] = {0};
    uint8_t data[PAGE_SIZE * 2] = {0XA5, 0XA5, 0XA5, 0XA5, 0XA5, 0XA5, 0XA5, 0XA5, 0XA5, 0XA5, 0XA5, 0XA5, 0XA5, 0XA5, 0XA5, 0XA5,
                                   0XAC, 0XAC, 0XAC, 0XAC, 0XAC, 0XAC, 0XAC, 0XAC, 0XAC, 0XAC, 0XAC, 0XAC, 0XAC, 0XAC, 0XAC, 0XAC};
    // 0XAC,0XAC,0XAC,0XAC,0XAC,0XAC,0XAC,0XAC,0XAC,0XAC,0XAC,0XAC,0XAC,0XAC,0XAC,0XAC
    // 0XA5,0XA5,0XA5,0XA5,0XA5,0XA5,0XA5,0XA5,0XA5,0XA5,0XA5,0XA5,0XA5,0XA5,0XA5,0XA5

    AT24CXX_Write(0, data, 5);
    Delay_ms(5);
    printf("写入5个数据...\r\n");
    AT24CXX_Read(0, temp, 5);
    if (temp[0] != judge)
    {
        printf("AT24CXX NG!\r\n");
    }
    else
    {
        printf("读取写入数据组1!\r\n");
        for (uint8_t i = 0; i < sizeof(temp) / sizeof(temp[0]); i++)
        {
            printf("%#02X ", temp[i]);
            if (i > 0 & (i + 1) % 16 == 0)
                printf("\r\n");
        }
    }

    AT24CXX_Write(240, data, 16);
    Delay_ms(5);
    printf("\r\n写入16个数据...\r\n");
    AT24CXX_Read(240, temp, 16);
    if (temp[0] != judge)
    {
        printf("AT24CXX NG!\r\n");
    }
    else
    {
        printf("读取写入数据组2!\r\n");
        for (uint8_t i = 0; i < sizeof(temp) / sizeof(temp[0]); i++)
        {
            printf("%#02X ", temp[i]);
            if (i > 0 & (i + 1) % 16 == 0)
                printf("\r\n");
        }
    }

    AT24CXX_Write(240, data, 20);
    Delay_ms(5);
    printf("\r\n写入20个数据...\r\n");
    AT24CXX_Read(240, temp, 20);
    if (temp[0] != judge)
    {
        printf("AT24CXX NG!\r\n");
    }
    else
    {
        printf("读取写入数据组3!\r\n");
        for (uint8_t i = 0; i < sizeof(temp) / sizeof(temp[0]); i++)
        {
            printf("%#02X ", temp[i]);
            if (i > 0 & (i + 1) % 16 == 0)
                printf("\r\n");
        }
    }

    AT24CXX_Write(240, data, 32);
    Delay_ms(5);
    printf("\r\n写入32个数据...\r\n");
    AT24CXX_Read(240, temp, 32);
    if (temp[0] != judge)
    {
        printf("AT24CXX NG!\r\n");
    }
    else
    {
        printf("读取写入数据组4!\r\n");
        for (uint8_t i = 0; i < sizeof(temp) / sizeof(temp[0]); i++)
        {
            printf("%#02X ", temp[i]);
            if (i > 0 & (i + 1) % 16 == 0)
                printf("\r\n");
        }
    }

    AT24CXX_Write(245, data, 32);
    Delay_ms(5);
    printf("\r\n偏移5个地址写入32个数据...\r\n");
    AT24CXX_Read(245, temp, 32);
    if (temp[0] != judge)
    {
        printf("AT24CXX NG!\r\n");
    }
    else
    {
        printf("读取写入数据组5!\r\n");
        for (uint8_t i = 0; i < sizeof(temp) / sizeof(temp[0]); i++)
        {
            printf("%#02X ", temp[i]);
            if (i > 0 & (i + 1) % 16 == 0)
                printf("\r\n");
        }
    }
}