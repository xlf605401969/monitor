#include "CANManager.h"
#include "CANQueue.h"
#include "CANString.h"
#include "ControlStruct.h"
#include <stdlib.h>
#include <string.h>

int IsCANBusy = 0;
int RecvFlag = 0;

char CommandBuffer[100];
char ReplyCommandBuffer[100];

void ReadCommand()
{
	char c;
	int i = 0;
	while (c = DequeueRecv() != EOF)
	{
		CommandBuffer[i] = c;
		i++;
	}
	CommandBuffer[i] = '\0';
}

void HandleCommand()
{
	switch (CommandBuffer[0])
	{
	case('R'):
		HandleR();
		break;
	case('F'):
		HandleF();
		break;
	case('H'):
		HandleH();
		break;
	case('M'):
		HandleM();
		break;
	case('S'):
		HandleS();
	default:
		break;
	}
}

void HandleR()
{
	switch (code_value_int(CommandBuffer+1))
	{
	case(1):
		break;
	case(2):
		break;
	case(3):
		break;
	case(4):
		break;
	default:
		break;
	}
}

void HandleF()
{
	switch (code_value_int(CommandBuffer + 1))
	{
	case(2):
		break;
	default:
		break;
	}
}

void HandleH()
{
	switch (code_value_int(CommandBuffer + 1))
	{
	case(1):
		break;
	default:
		break;
	}
}

void HandleM()
{
	switch (code_value_int(CommandBuffer + 1))
	{
	case(0):
		break;
	default:
		break;
	}
}

void HandleS()
{

}

void HandleM0()
{
	int i;
	float v;
	CtrlDtLnkdLstElement* e;

	i = code_value_int(code_position(CommandBuffer, 'I'));
	v = code_value_float(code_position(CommandBuffer, 'V'));
	e = SelectLstElementByID(i);
	if (e != 0)
	{
		switch (e->Data.Type)
		{
		case(INT32):
			*(long*)(e->Data.Address) = (int)v;
			break;
		case(FLOAT):
			*(float*)(e->Data.Address) = v;
			break;
		default:
			break;
		}
	}
}

void R1ByID(int id)
{
	CtrlDtLnkdLstElement* e;
	e = SelectLstElementByID(id);
	if (e != 0)
	{
		R1(e);
	}
}

void R1(CtrlDtLnkdLstElement* e)
{
	char* p = ReplyCommandBuffer;
	strcpy(p, "R1 I");
	p = p + 4;
	ltoa(e->Data.ID, p, 10);
	p += strlen(p);
	*p = ' ';
	p++;
	*p = 'V';
	p++;
	*p = '\0';
	GetControlDataValue(e, p);
	p += strlen(p);
	*p = '\0';
}

void F1(int id)
{

}

void H1(int id)
{

}

char* GetControlDataValue(CtrlDtLnkdLstElement* e, char* buffer)
{
	switch (e->Data.Type)
	{
	case(INT32):
		ltoa(*(long*)(e->Data.Address), buffer, 10);
		break;
	case(FLOAT):
		ftoa(*(float*)(e->Data.Address), 3, buffer);
		break;
	default:
		break;
	}
	return buffer;
}