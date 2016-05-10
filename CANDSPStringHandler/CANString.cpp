#include "CANString.h"
#include <math.h>
#include <stdlib.h>
#include <string.h>

char* ftoa(float f, int de, char * str)
{
	int i, d = 1, temp;
	char* strs = str;
	i = floor(f);
	for (temp = de; temp > 0; temp--)
		d *= 10;
	d = (f - i)*d;
	ltoa(i, str, 10);
	str += strlen(str);
	*str = '.';
	str++;
	ltoa(d, str, 10);
	return strs;
}

char* code_position(char* str, char c)
{
	return strchr(str, c);
}

float code_value(char* str)
{
	return atof(str);
}