#ifndef _COMMAND_MANAGER_
#define _COMMAND_MANAGER_

typedef struct CommandStruct CommandStruct;
struct CommandStruct {
	long ID;
	char Name[24];
	void(*Execute)();
};

typedef struct CmdLnkdLstElement CmdLnkdLstElement;
struct CmdLnkdLstElement {
	CmdLnkdLstElement* Previous;
	CmdLnkdLstElement* Next;
	CommandStruct Command;
};

#endif

void CmdLstAppendElement(CmdLnkdLstElement * e);

void CmdLstRmTailElement();

void CmdLstRmElementByID(long id);

CmdLnkdLstElement * CmdLstSelectElementByID(long id);

void AddCommand(long id, char* name, void(*command)());

void ExecCommand(CmdLnkdLstElement * e);

void ExecCommandByID(long id);

CmdLnkdLstElement * CmdLstGetEntry();

CmdLnkdLstElement * CmdLstGetEnd();
