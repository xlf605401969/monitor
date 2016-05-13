#include "TimingTaskScheduler.h"
#include <stdlib.h>

TmngTskLnkdLstElement* TmngTskLnkdLstEntry = 0;
TmngTskLnkdLstElement* TmngTskLnkdLstEnd = 0;

unsigned long long TimingTaskTimerTick = 0;
unsigned int TimingTaskInc = 1000000 / TIMING_TASK_TIMER_FREQ;
int LoopServerExecutedFlag = 0;

void TskLstAppend(TmngTskLnkdLstElement* e)
{
	if (TmngTskLnkdLstEntry == 0)
	{
		TmngTskLnkdLstEntry = TmngTskLnkdLstEnd = e;
		e->Next = e->Previous = 0;
	}
	else
	{
		TmngTskLnkdLstEnd->Next = e;
		e->Previous = TmngTskLnkdLstEnd;
		e->Next = 0;
		TmngTskLnkdLstEnd = e;
	}
}

TmngTskLnkdLstElement* TskLstSelectByID(long id)
{
	TmngTskLnkdLstElement* e = TmngTskLnkdLstEntry;
	while (e != 0)
	{
		if (e->Task.ID == id)
			break;
		e = e->Next;
	}
	return e;
}

void TskLstRmByID(long id)
{
	TmngTskLnkdLstElement* p = TskLstSelectByID(id);
	if (p != 0)
	{
		if (p->Previous != 0)
		{
			p->Previous->Next = p->Next;
		}
		else
		{
			TmngTskLnkdLstEntry = p->Next;
		}
		if (p->Next != 0)
		{
			p->Next->Previous = p->Previous;
		}
		else
		{
			TmngTskLnkdLstEnd = p->Previous;
		}
		free(p);
	}
}

int AddTimingTask(long id, long span, void* para, void(*body)(void*))
{
	if (TskLstSelectByID(id) == 0)
	{
		TmngTskLnkdLstElement* e = (TmngTskLnkdLstElement*)malloc(sizeof(TmngTskLnkdLstElement));
		e->Task.ID = id;
		e->Task.Para = para;
		e->Task.TimeSpan = span * 1000;
		e->Task.Body = body;
		TskLstAppend(e);
		return 0;
	}
	else
		return -1;
}

void RemoveTimingTask(long id)
{
	TskLstRmByID(id);
}

void TimingTaskTimerServer()
{
	TimingTaskTimerTick += TimingTaskInc;
	LoopServerExecutedFlag = 0;
}

void TimingTaskLoopServer()
{
	if (LoopServerExecutedFlag == 0)
	{
		TmngTskLnkdLstElement* e = TmngTskLnkdLstEntry;
		while (e != 0)
		{
			if (TimingTaskTimerTick % e->Task.TimeSpan == 0)
			{
				e->Task.Body(e->Task.Para);
			}
			e = e->Next;
		}
		LoopServerExecutedFlag = 1;
	}
}

void InitTaskScheduler()
{
	TmngTskLnkdLstEntry = 0;
	TmngTskLnkdLstEnd = 0;
}
