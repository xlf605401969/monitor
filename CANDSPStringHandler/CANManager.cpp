#include "CANManager.h"
#include "CANQueue.h"
#include "CANString.h"
#include "ControlStruct.h"
#include <stdlib.h>
#include <string.h>

long IsCANBusy = 0;
long RecvFlag = 0;

char RecvCommandBuffer[100] = { '\0' };
char SendCommandBuffer[100] = { '\0' };

void ReadCommand()
{
	char c;
	long i = 0;
	while ((c = DequeueRecv()) != EOF_C)
	{
		RecvCommandBuffer[i] = c;
		i++;
	}
	RecvCommandBuffer[i] = '\0';
}

void HandleCommand()
{
	switch (RecvCommandBuffer[0])
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
	switch (code_value_int32(RecvCommandBuffer))
	{
	case(0):
		HandleR0();
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

void HandleR0()
{
	long i;
	CtrlDtLnkdLstElement* e;

	i = code_value_int32(code_position(RecvCommandBuffer, 'I'));
	e = SelectLstElementByID(i);
	if (e != 0)
	{
		R1(e);
		EnqueueSend_String(SendCommandBuffer);
		EnqueueSendEOF();
	}
}

void HandleF()
{
	switch (code_value_int32(RecvCommandBuffer))
	{
	case(2):
		HandleF2();
		break;
	default:
		break;
	}
}

void HandleF2()
{
	CtrlDtLnkdLstElement* e = GetLnkdLstEntry();
	while (e != 0)
	{
		F1(e);
		EnqueueSend_String(SendCommandBuffer);
		EnqueueSendEOF();
		e = e->Next;
	}
}

void HandleH()
{
	switch (code_value_int32(RecvCommandBuffer + 1))
	{
	case(0):
		break;
	default:
		break;
	}
}

void HandleM()
{
	switch (code_value_int32(RecvCommandBuffer + 1))
	{
	case(0):
		HandleM0();
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
	long i, vi;
	float vf;
	CtrlDtLnkdLstElement* e;

	i = code_value_int32(code_position(RecvCommandBuffer, 'I'));
	e = SelectLstElementByID(i);
	if (e != 0)
	{
		switch (e->Data.Type)
		{
		case(INT32):
			vi = code_value_int32(code_position(RecvCommandBuffer, 'V'));
			*(long*)(e->Data.Address) = (long)vi;
			break;
		case(FLOAT):
			vf = code_value_float(code_position(RecvCommandBuffer, 'V'));
			*(float*)(e->Data.Address) = vf;
			break;
		default:
			break;
		}
		R1(e);
		EnqueueSend_String(SendCommandBuffer);
		EnqueueSendEOF();
	}
}

void R1ByID(long id)
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
	char* p = SendCommandBuffer;
	strcpy(p, "R1 I");
	p = p + 4;
	ltoa_dec(e->Data.ID, p);
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

void F1ByID(long id)
{
	CtrlDtLnkdLstElement* e;
	e = SelectLstElementByID(id);
	if (e != 0)
	{
		F1(e);
	}
}

void F1(CtrlDtLnkdLstElement* e)
{
	char* p = SendCommandBuffer;
	strcpy(p, "F1 I");
	p = p + 4;
	ltoa_dec(e->Data.ID, p);
	p += strlen(p);
	*p = ' ';
	p++;
	*p = 'T';
	p++;
	ltoa_dec(e->Data.Type, p);
	p += strlen(p);
	*p = ' ';
	p++;
	*p = 'W';
	p++;
	ltoa_dec(e->Data.IsEditable, p);
	p += strlen(p);
	*p = ' ';
	p++;
	strcpy(p, e->Data.Name);
}

void H1(long id)
{
	char* p = SendCommandBuffer;
	strcpy(p, "H1");
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