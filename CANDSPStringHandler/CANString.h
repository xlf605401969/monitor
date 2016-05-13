#ifndef _CAN_STRING_
#define _CAN_STRING_

char* ftoa(float f, long de, char * str);

char * code_position(char * str, char c);

float code_value_float(char * str);

long code_value_int32(char * str);

char * ltoa_dec(long value, char * buffer);

long roundl_c(float value);

char *ltoa_c(long value, char *string, int radix);

#endif
