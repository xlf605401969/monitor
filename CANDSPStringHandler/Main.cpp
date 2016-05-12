#include "CANString.h"
#include <stdio.h>
#include "ControlStruct.h"
#include "CANManager.h"
#include "CANQueue.h"
#include "CommandManager.h"
#include "TimingTaskScheduler.h"
#include <string.h>
#include <Windows.h>

void TestCommand();
void TestTask(void*);

float i, j, k;
long main()
{
	char str[100] = "R3 I1 T1000";
	AddCtrlData(1, DTFLOAT, 1, "hahaha", &i);
	AddCtrlData(2, DTFLOAT, 1, "hahaha", &j);
	AddCommand(128, "TestCommand", TestCommand);
	AddTimingTask(1, 1000, 0, TestTask);

	while (1)
	{
		EnqueueRecv_String(str);
		EnqueueRecvEOF();
		ReadCommand();
		HandleCommand();
		while (SendQueueLength() > 0)
		{
			char c = DequeueSend();
			printf("%c", c);
		}
		printf("\n");
		TimingTaskLoopServer();
		TimingTaskTimerServer();
		Sleep(10);
	}
}

void TestCommand()
{
	printf("Executing Command!\n");
}

void TestTask(void *)
{
	printf("Executing Timing Task!\n");
}

