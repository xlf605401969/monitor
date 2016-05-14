#ifndef _CONTROL_STRUCT_
#define _CONTROL_STRUCT_

enum ControlDataType {
	DTNONE,
	DTINT32,
	DTFLOAT,
	DTINT16
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


void CtrlDtLstAppend(CtrlDtLnkdLstElement* e);

void CtrlDtLstRmTail();

void CtrlDtLstRmByID(long id);

CtrlDtLnkdLstElement* CtrlDtLstSelectByID(long id);

void AddCtrlData(long id, ControlDataType type, long isEditable, char* name, void* address);

CtrlDtLnkdLstElement * CtrlDtLnkdLstGetEntry();

CtrlDtLnkdLstElement * CtrlDtLnkdLstGetEnd();

void InitControlStruct();

#endif
