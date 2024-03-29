#include "CANManager.h"
#include "CANQueue.h"
#include "CANString.h"
#include "ControlStruct.h"
#include "CommandManager.h"
#include "TimingTaskScheduler.h"
#include "CANDriver.h"
#include <stdlib.h>
#include <string.h>

char RecvCommandBuffer[100] = { '\0' };
char SendCommandBuffer[100] = { '\0' };

void ReadCommand()
{
	char c;
	long i = 0;
	while ((c = DequeueRecv()) != EOF_C && i < 100)
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
		HandleR2();
		break;
	case(3):
		HandleR3();
		break;
	case(4):
		HandleR4();
		break;
	case(5):
		HandleR5();
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
	e = CtrlDtLstSelectByID(i);
	if (e != 0)
	{
		R1(e);
		EnqueueSend_String(SendCommandBuffer);
	}
}

void HandleR2()
{
	CtrlDtLnkdLstElement* e = CtrlDtLnkdLstGetEntry();
	while(e != 0)
	{
		R1(e);
		EnqueueSend_String(SendCommandBuffer);
		e = e->Next;
	}
}

void HandleR3()
{
	long i, t;
	CtrlDtLnkdLstElement* e;
	TmngTskLnkdLstElement* et;

	i = code_value_int32(code_position(RecvCommandBuffer, 'I'));
	t = code_value_int32(code_position(RecvCommandBuffer, 'T'));
	e = CtrlDtLstSelectByID(i);
	if (e != 0)
	{
		et = TskLstSelectByID(i + REPORT_TASK_OFFSET);
		if (et == 0)
			AddTimingTask(i + REPORT_TASK_OFFSET, t, (void*)e, ReportTask);
		else
			et->Task.TimeSpan = t;
	}
}

void HandleR4()
{
	long i = code_value_int32(code_position(RecvCommandBuffer, 'I'));
	RemoveTimingTask(i + REPORT_TASK_OFFSET);
}

void HandleR5()
{
	TmngTskLnkdLstElement* e = TmngTskLnkdLstGetEntry();
	TmngTskLnkdLstElement* etemp = e;
	while(e != 0)
	{
		etemp = e;
		e = e->Next;
		if (etemp->Task.ID > REPORT_TASK_OFFSET)
		{
			TskLstRm(etemp);
		}
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
	CtrlDtLnkdLstElement* e = CtrlDtLnkdLstGetEntry();
	while (e != 0)
	{
		F1(e);
		EnqueueSend_String(SendCommandBuffer);
		e = e->Next;
	}

	CmdLnkdLstElement* e1 = CmdLstGetEntry();
	while (e1 != 0)
	{
		F3(e1);
		EnqueueSend_String(SendCommandBuffer);
		e1 = e1->Next;
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
	long i;
	i = code_value_int32(code_position(RecvCommandBuffer, 'S'));
	HandleSN(i);
}

void HandleSN(long id)
{
	ExecCommandByID(id);
}

void HandleM0()
{
	long i, vi;
	float vf;
	CtrlDtLnkdLstElement* e;

	i = code_value_int32(code_position(RecvCommandBuffer, 'I'));
	e = CtrlDtLstSelectByID(i);
	if (e != 0)
	{
		switch (e->Data.Type)
		{
		case(DTINT32):
			vi = code_value_int32(code_position(RecvCommandBuffer, 'V'));
			*(long*)(e->Data.Address) = (long)vi;
			break;
		case(DTFLOAT):
			vf = code_value_float(code_position(RecvCommandBuffer, 'V'));
			*(float*)(e->Data.Address) = vf;
			break;
		case(DTINT16):
			vi = code_value_int32(code_position(RecvCommandBuffer, 'V'));
			*(int*)(e->Data.Address) = (int)vi;
			break;
		default:
			break;
		}
		R1(e);
		EnqueueSend_String(SendCommandBuffer);
	}
}

void R1ByID(long id)
{
	CtrlDtLnkdLstElement* e;
	e = CtrlDtLstSelectByID(id);
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
	e = CtrlDtLstSelectByID(id);
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
	*p = 'N';
	p++;
	strcpy(p, e->Data.Name);
}

void F3(CmdLnkdLstElement* e)
{
	char* p = SendCommandBuffer;
	strcpy(p, "F3 I");
	p = p + 4;
	ltoa_dec(e->Command.ID, p);
	p += strlen(p);
	*p = ' ';
	p++;
	*p = 'N';
	p++;
	strcpy(p, e->Command.Name);
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
	case(DTINT32):
		ltoa_dec(*(long*)(e->Data.Address), buffer);
		break;
	case(DTFLOAT):
		ftoa(*(float*)(e->Data.Address), 3, buffer);
		break;
	case(DTINT16):
		ltoa_dec(*(int*)(e->Data.Address), buffer);
		break;
	default:
		break;
	}
	return buffer;
}

void ReportTask(void* data)
{
	CtrlDtLnkdLstElement* e = (CtrlDtLnkdLstElement*)data;
	R1(e);
	EnqueueSend_String(SendCommandBuffer);
}

void CANManagerTask(void* d)
{
	if (RecvEOFFlag)
	{
		RecvEOFFlag--;
		ReadCommand();
		HandleCommand();
	}
	if (!IsCANBusy)
	{
		CAN_TRY_SEND();
	}
}

void InitCANManager()
{
	AddTimingTask(1, 30, (void*)0, CANManagerTask);
	AddCtrlData(1, DTFLOAT, 0, "CPU_LOAD", &CPULoad);
	RecvEOFFlag = 0;
}
