#include "CANString.h"
#include <stdio.h>
#include "ControlStruct.h"
#include "CANManager.h"
#include "CANQueue.h"
#include "CommandManager.h"
#include <string.h>

void TestCommand();

float i, j, k;
long main()
{
	char str[100] = "F2";
	AddCtrlData(1, FLOAT, 1, "hahaha", &i);
	AddCtrlData(2, FLOAT, 1, "hahaha", &j);
	AddCommand(128, "TestCommand", TestCommand);

	while (1)
	{
		getchar();
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
	}
}

void TestCommand()
{
	printf("Executing Command!\n");
}

