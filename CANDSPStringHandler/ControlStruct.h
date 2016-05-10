#pragma once

typedef enum ControlDataType ControlDataType;
enum ControlDataType{
	NONE,
	INT16,
	FLOAT,
};

typedef struct ControlData ControlData;
struct ControlData{
	char Name[16];
	int ID;
	ControlDataType Type;
	void* Address;
	int IsEditable;
};

typedef struct CtrlDtLnkdLstElement CtrlDtLnkdLstElement;
struct CtrlDtLnkdLstElement{
	CtrlDtLnkdLstElement* Previous;
	ControlData Data;
	CtrlDtLnkdLstElement* Next;
};

void LstAppendElement(CtrlDtLnkdLstElement * e);

void LstRmTailElement();

void LstRmElementByID(int id);

CtrlDtLnkdLstElement * SelectLstElementByID(int id);

void AddCtrlData(int id, ControlDataType type, int isEditable, char * name, void * address);
