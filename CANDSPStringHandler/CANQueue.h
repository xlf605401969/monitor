#ifndef _CAN_QUEUE_
#define _CAN_QUEUE_

#define SEND_BUFFER_SIZE 1024
#define RECV_BUFFER_SIZE 1024
#define EOF_C (char)0xff

void EnqueueSend(char c);

char DequeueSend();

void EnqueueRecv(char c);

char DequeueRecv();

long SendQueueLength();

long RecvQueueLength();

void EnqueueSendEOF();

void EnqueueRecvEOF();

void EnqueueSend_String(char * str);

void EnqueueRecv_String(char * str);

void InitCANQueue();

#endif
