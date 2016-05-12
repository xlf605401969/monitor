#include "CANQueue.h"
#include <string.h>
#include <stdio.h>
#include <stdlib.h>

char SendBuffer[SEND_BUFFER_SIZE];
char RecvBuffer[RECV_BUFFER_SIZE];

long SendTail = 0;
long SendHead = 0;
long RecvTail = 0;
long RecvHead = 0;

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

void EnqueueRecv(char c)
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

long SendQueueLength()
{
	return (SendTail - SendHead) % SEND_BUFFER_SIZE;
}

long RecvQueueLength()
{
	return (RecvTail - RecvHead) % RECV_BUFFER_SIZE;
}

void EnqueueSendEOF()
{
	EnqueueSend(EOF_C);
}

void EnqueueRecvEOF()
{
	EnqueueRecv(EOF_C);
}

void EnqueueSend_String(char* str)
{
	long length = strlen(str);
	if (SEND_BUFFER_SIZE - SendTail > length)
	{
		strcpy(&SendBuffer[SendTail], str);
		SendTail += length;
	}
	else
	{
		long tempLength = SEND_BUFFER_SIZE - SendTail + 1;
		memcpy(&SendBuffer[SendTail], str, tempLength);
		SendTail = 0;
		memcpy(SendBuffer, str + tempLength, length - tempLength);
		SendTail += (length - tempLength);
	}
}

void EnqueueRecv_String(char* str)
{
	long length = strlen(str);
	if (RECV_BUFFER_SIZE - RecvTail > length)
	{
		strcpy(&RecvBuffer[RecvTail], str);
		RecvTail += length;
	}
	else
	{
		long tempLength = RECV_BUFFER_SIZE - RecvTail + 1;
		memcpy(&RecvBuffer[RecvTail], str, tempLength);
		RecvTail = 0;
		memcpy(RecvBuffer, str + tempLength, length - tempLength);
		RecvTail += (length - tempLength);
	}
}



