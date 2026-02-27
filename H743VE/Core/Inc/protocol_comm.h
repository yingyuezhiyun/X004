#pragma once
#include "stdint.h"

#define MAX_SIZE (256)
#define CMD_HEAD (0x545A)
#define CMD_TAIL (0xFE5A)
#define PC_ID (0x14)
#define MCU_ID (0x1c)

#define PC_ACK(cmd, data) pc_send_ack(uart_para, cmd, &data, sizeof(data))

#define PC_ACK_INT16(cmd, data)                               \
    {                                                         \
        int16_t data1 = (int16_t)round(data);                 \
        pc_send_ack(uart_para, cmd, &data1, sizeof(int16_t)); \
    }

#define PC_ACK_UINT16(cmd, data)                               \
    {                                                          \
        uint16_t data1 = (uint16_t)round(data);                \
        pc_send_ack(uart_para, cmd, &data1, sizeof(uint16_t)); \
    }

#define SET_PARAM_INT16(param) (param) = *(int16_t *)data
#define SET_PARAM_INT8(param) (param) = *(int8_t *)data

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


typedef void (*CommandFunction)(uart_para_t *uart_para, uint8_t cmd, uint8_t *data, size_t data_len);

