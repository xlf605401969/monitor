#ifndef _CAN_MANAGER_
#define _CAN_MANAGER_

#include "ControlStruct.h"

extern char RecvCommandBuffer[100];
extern char SendCommandBuffer[100];

void R1ByID(long id);

void R1(CtrlDtLnkdLstElement* e);

void F1ByID(long id);

void F1(CtrlDtLnkdLstElement* e);

void H1(long id);

char* GetControlDataValue(CtrlDtLnkdLstElement* e, char* buffer);

void ReadCommand();

void HandleCommand();

void HandleR();

void HandleR0();

void HandleF();

void HandleF2();

void HandleH();

void HandleM();

void HandleS();

void HandleM0();

#endif
