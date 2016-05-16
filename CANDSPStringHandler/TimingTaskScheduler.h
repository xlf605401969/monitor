#ifndef _TIMING_TASK_SCHEDULER_
#define _TIMING_TASK_SCHEDULER_

#define TIMING_TASK_TIMER_FREQ 10000
#define CPU_LOAD_COUNTER_1S 5200000

extern float CPULoad;

typedef struct TimingTask TimingTask;
struct TimingTask {
	long ID;
	unsigned long long TimeSpan;
	unsigned long long Tick;
	void* Para;
	void(*Body)(void*);
};

typedef struct TmngTskLnkdLstElement TmngTskLnkdLstElement;
struct TmngTskLnkdLstElement {
	TmngTskLnkdLstElement* Previous;
	TmngTskLnkdLstElement* Next;
	TimingTask Task;
};


void TskLstAppend(TmngTskLnkdLstElement * e);

TmngTskLnkdLstElement * TskLstSelectByID(long id);

void TskLstRmByID(long id);

void TskLstRm(TmngTskLnkdLstElement* p);

int AddTimingTask(long id, long span, void * para, void(*body)(void *));

void RemoveTimingTask(long id);

TmngTskLnkdLstElement* TmngTskLnkdLstGetEntry();

void TimingTaskTimerServer();

void TimingTaskLoopServer();

void InitTaskScheduler();

void CPULoadCalculationTask();

#endif

