#include "CANQueue.h"
#include <string.h>
#include <stdio.h>
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
	return r;
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
	return r;
}

int SendQueueLength()
{
	return (SendTail - SendHead) % SEND_BUFFER_SIZE;
}

int RecvQueueLength()
{
	return (RecvTail - RecvHead) % RECV_BUFFER_SIZE;
}

void EnqueueSendEOF()
{
	EnqueueSend(0xff);
}

void EnqueueSend_String(char* str)
{
	int length = strlen(str);
	if (SEND_BUFFER_SIZE - SendTail < length)
	{
		strcpy(&SendBuffer[SendTail], str);
		SendTail += length;
	}
	else
	{
		int tempLength = SEND_BUFFER_SIZE - SendTail + 1;
		memcpy(&SendBuffer[SendTail], str, tempLength);
		SendTail = 0;
		memcpy(SendBuffer, str + tempLength, length - tempLength);
		SendTail += (length - tempLength);
	}
}



