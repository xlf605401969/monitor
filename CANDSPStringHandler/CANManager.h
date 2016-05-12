#ifndef _CAN_MANAGER_
#define _CAN_MANAGER_

#include "ControlStruct.h"
#include "CommandManager.h"

#define REPORT_TASK_OFFSET 100

extern char RecvCommandBuffer[100];
extern char SendCommandBuffer[100];

void R1ByID(long id);

void R1(CtrlDtLnkdLstElement* e);

void F1ByID(long id);

void F1(CtrlDtLnkdLstElement* e);

void F3(CmdLnkdLstElement * e);

void H1(long id);

char* GetControlDataValue(CtrlDtLnkdLstElement* e, char* buffer);

void ReportTask(void * data);

void CANManagerTask(void *);

void InitCANManager();

void ReadCommand();

void HandleCommand();

void HandleR();

void HandleR0();

void HandleR3();

void HandleR4();

void HandleF();

void HandleF2();

void HandleH();

void HandleM();

void HandleS();

void HandleSN(long id);

void HandleM0();

#endif
