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
	ltoa_c(i, str, 10);
	str += strlen(str);
	*str = '.';
	str++;
	ltoa_c(d, str, 10);
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
	return ltoa_c(value, buffer, 10);
}

char* ltoa_c(long value, char *string, int radix)
{
	char tmp[33];
	char *tp = tmp;
	long i;
	unsigned long v;
	int sign;
	char *sp;

	if (radix > 36 || radix <= 1)
	{
		return 0;
	}

	sign = (radix == 10 && value < 0);
	if (sign)
		v = -value;
	else
		v = (unsigned long)value;
	while (v || tp == tmp)
	{
		i = v % radix;
		v = v / radix;
		if (i < 10)
			*tp++ = i + '0';
		else
			*tp++ = i + 'a' - 10;
	}

	if (string == 0)
		string = (char *)malloc((tp - tmp) + sign + 1);
	sp = string;

	if (sign)
		*sp++ = '-';
	while (tp > tmp)
		*sp++ = *--tp;
	*sp = 0;
	return string;
}

