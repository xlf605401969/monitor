#ifndef _CONTROL_STRUCT_
#define _CONTROL_STRUCT_

enum ControlDataType {
	NONE,
	INT32,
	FLOAT
};
typedef enum ControlDataType ControlDataType;


typedef struct ControlData ControlData;
struct ControlData{
	char Name[16];
	long ID;
	ControlDataType Type;
	void* Address;
	long IsEditable;
};


typedef struct CtrlDtLnkdLstElement CtrlDtLnkdLstElement;
struct CtrlDtLnkdLstElement{
	CtrlDtLnkdLstElement* Previous;
	ControlData Data;
	CtrlDtLnkdLstElement* Next;
};


void CtrlDtLstAppendElement(CtrlDtLnkdLstElement* e);

void CtrlDtLstRmTailElement();

void CtrlDtLstRmElementByID(long id);

CtrlDtLnkdLstElement* CtrlDtLstSelectElementByID(long id);

void AddCtrlData(long id, ControlDataType type, long isEditable, char* name, void* address);

CtrlDtLnkdLstElement * CtrlDtLnkdLstGetEntry();

CtrlDtLnkdLstElement * CtrlDtLnkdLstGetEnd();

#endif
