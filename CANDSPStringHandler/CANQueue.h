#pragma once
#define SEND_BUFFER_SIZE 1024
#define RECV_BUFFER_SIZE 1024

void EnqueueSend(char c);

char DequeueSend();

void EqueueRecv(char c);

char DequeueRecv();

int SendQueueLength();

int RecvQueueLength();

void EnqueueSendEOF();

void EnqueueSend_String(char * str);
