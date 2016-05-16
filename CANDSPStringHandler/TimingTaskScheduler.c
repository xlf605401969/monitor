#include "TimingTaskScheduler.h"
#include <stdlib.h>

TmngTskLnkdLstElement* TmngTskLnkdLstEntry = 0;
TmngTskLnkdLstElement* TmngTskLnkdLstEnd = 0;

unsigned long long TimingTaskTimerTick = 0;
unsigned long long TimingTaskTimerTickTemp = 0;
unsigned long long TimingTaskTimerTickTempOld = 0;
unsigned long long TimingTaskTimerTickPassed = 0;
unsigned int TimingTaskInc = 1000000 / TIMING_TASK_TIMER_FREQ;
unsigned long CPULoadCounter;
float CPULoadCounter1sScaler = 1.0/CPU_LOAD_COUNTER_1S;
float CPULoad;

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
	TskLstRm(p);
}

void TskLstRm(TmngTskLnkdLstElement* p)
{
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
		e->Task.Tick = 0;
		e->Task.Body = body;
		TskLstAppend(e);
		return 0;
	}
	else
		return -1;
}

TmngTskLnkdLstElement* TmngTskLnkdLstGetEntry()
{
	return TmngTskLnkdLstEntry;
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
	TimingTaskTimerTickTempOld = TimingTaskTimerTickTemp;
	TimingTaskTimerTickTemp = TimingTaskTimerTick;
	TimingTaskTimerTickPassed = TimingTaskTimerTickTemp - TimingTaskTimerTickTempOld;
	if (LoopServerExecutedFlag == 0)
	{
		TmngTskLnkdLstElement* e = TmngTskLnkdLstEntry;
		while (e != 0)
		{
			e->Task.Tick += TimingTaskTimerTickPassed;
			if (e->Task.Tick >= e->Task.TimeSpan)
			{
				e->Task.Body(e->Task.Para);
				e->Task.Tick = 0;
			}
			e = e->Next;
		}
		LoopServerExecutedFlag = 1;
	}
	CPULoadCounter++;
}

void InitTaskScheduler()
{
	TmngTskLnkdLstEntry = 0;
	TmngTskLnkdLstEnd = 0;
	CPULoadCounter = 0;
	AddTimingTask(0, 1000, (void*)0, CPULoadCalculationTask);
}

void CPULoadCalculationTask()
{
	CPULoad = (CPU_LOAD_COUNTER_1S - CPULoadCounter) * CPULoadCounter1sScaler;
	CPULoadCounter = 0;
}
