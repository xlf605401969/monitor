#include "CANQueue.h"
#include <string.h>
#include <stdlib.h>

char SendBuffer[SEND_BUFFER_SIZE];
char RecvBuffer[RECV_BUFFER_SIZE];

int SendTail = 0;
int SendHead = 0;
int RecvTail = 0;
int RecvHead = 0;

void EnqueueSend(char c)
{
	SendBuffer[SendTail] = c;
	SendTail++;
	SendTail %= SEND_BUFFER_SIZE;
}

char DequeueSend()
{
	char r = SendBuffer[SendHead];
	SendHead++;
	SendHead %= SEND_BUFFER_SIZE;
}

void EqueueRecv(char c)
{
	RecvBuffer[RecvTail] = c;
	RecvTail++;
	RecvTail %= RECV_BUFFER_SIZE;
}

char DequeueRecv()
{
	char r = RecvBuffer[RecvHead];
	RecvHead++;
	RecvHead %= RECV_BUFFER_SIZE;
}