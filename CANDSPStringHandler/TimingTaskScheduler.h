#ifndef _TIMING_TASK_SCHEDULER_
#define _TIMING_TASK_SCHEDULER_

#define TIMING_TASK_TIMER_FREQ 100

typedef struct TimingTask TimingTask;
struct TimingTask {
	long ID;
	long long TimeSpan;
	void* Para;
	void(*Body)(void*);
};

typedef struct TmngTskLnkdLstElement TmngTskLnkdLstElement;
struct TmngTskLnkdLstElement {
	TmngTskLnkdLstElement* Previous;
	TmngTskLnkdLstElement* Next;
	TimingTask Task;
};



#endif

void TskLstAppend(TmngTskLnkdLstElement * e);

TmngTskLnkdLstElement * TskLstSelectByID(long id);

void TskLstRmByID(long id);

int AddTimingTask(long id, long span, void * para, void(*body)(void *));

void RemoveTimingTask(long id);

void TimingTaskTimerServer();

void TimingTaskLoopServer();
