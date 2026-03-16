#pragma once
#include "stdint.h"

#include "global_cfg.h"

#define MAX_SIZE (256)
#define CMD_HEAD (0x545A)
#define CMD_TAIL (0xFE5A)
#define PC_ID (0x14)
#define MCU_ID (0x1c)

#define PC_ACK(cmd, data) send_ack(huart, cmd, &data, sizeof(data))

#define PC_ACK_INT16(cmd, data)                        \
    {                                                  \
        int16_t data1 = (int16_t)round(data);          \
        send_ack(huart, cmd, &data1, sizeof(int16_t)); \
    }

#define PC_ACK_UINT16(cmd, data)                        \
    {                                                   \
        uint16_t data1 = (uint16_t)round(data);         \
        send_ack(huart, cmd, &data1, sizeof(uint16_t)); \
    }

#define SET_PARAM_INT16(param) (param) = *(int16_t *)data
#define SET_PARAM_INT8(param) (param) = *(int8_t *)data

#define SET_SW_STA(param)                        \
    {                                            \
        uint8_t type = *(uint8_t *)data;         \
        if (type == WORK_OFF || type == WORK_ON) \
            param = type;                        \
    }

// #define SET_PULSE_TYPE(param)                    \
//     {                                            \
//         uint8_t type = *(uint8_t *)data;         \
//         if (type == WORK_OFF || type == WORK_ON) \
//         {                                        \
//             param = type;                        \
//         }                                        \
//     }

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

typedef struct
{
    uint16_t Num;
    uint16_t Interval[11]; // 单位 us 范围 240-660
} Pulse_param_packet_t;

typedef void (*CommandFunction)(UART_HandleTypeDef *huart, uint8_t cmd, uint8_t *data, size_t data_len);

void parse_and_execute_command(uart_para_t *uart_para, CommandFunction exec_commands);




extern char Version2[];
