#include "CANString.h"
#include <math.h>
#include <stdlib.h>
#include <string.h>

char* ftoa(float f, long de, char * str)
{
	long i, d = 1, temp;
	char* strs = str;
	i = (long)floor(f);
	for (temp = de; temp > 0; temp--)
		d *= 10;
	d = (long)ceil((f - i)*d);
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

float code_value_float(char* str)
{
	return (float)atof(str + 1);
}

long code_value_int32(char* str)
{
	return atol(str + 1);
}

char* ltoa_dec(long value, char* buffer)
{
	return ltoa(value, buffer, 10);
}

