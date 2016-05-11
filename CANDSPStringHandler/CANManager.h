#pragma once
#include "ControlStruct.h"

extern char CommandBuffer[100];
extern char ReplyCommandBuffer[100];

void R1ByID(int id);

void R1(CtrlDtLnkdLstElement * e);

void F1(int id);

void H1(int id);

char * GetControlDataValue(CtrlDtLnkdLstElement * e, char * buffer);

void ReadCommand();

void HandleCommand();

void HandleR();

void HandleF();

void HandleH();

void HandleM();

void HandleS();

void HandleM0();
