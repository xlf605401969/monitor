#include "CANString.h"
#include <stdio.h>
#include "ControlStruct.h"
#include "CANManager.h"
#include "CANQueue.h"
#include <string.h>

float i, j, k;
long main()
{
	char str[100] = "F2 I1";
	AddCtrlData(1, FLOAT, 1, "hahaha", &i);
	AddCtrlData(2, FLOAT, 1, "hahaha", &j);
	while (1)
	{
		scanf("%f", &i);
		EnqueueRecv_String(str);
		EnqueueRecvEOF();
		ReadCommand();
		HandleCommand();
		printf(SendCommandBuffer);
		printf("\n");
	}
}

