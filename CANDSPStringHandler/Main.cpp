#include "CANString.h"
#include <stdio.h>
#include "ControlStruct.h"

float i, j, k;
int main()
{
	char str[100];
	AddCtrlData(1, FLOAT, 1, "hahaha", &i);
	AddCtrlData(2, FLOAT, 1, "hahaha", &j);
	while (1)
	{
		scanf("%f", &i);
		printf(ftoa(*(float*)((SelectLstElementByID(2)->Data).Address),3,str));

	}
}