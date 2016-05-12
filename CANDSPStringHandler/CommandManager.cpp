#include "CommandManager.h"
#include <stdlib.h>
#include <string.h>

CmdLnkdLstElement* CmdLnkdLstEntry = 0;
CmdLnkdLstElement* CmdLnkdLstEnd = 0;

void CmdLstAppendElement(CmdLnkdLstElement* e)
{
	if (CmdLnkdLstEntry == 0)
	{
		CmdLnkdLstEntry = e;
		e->Next = 0;
		e->Previous = 0;
	}
	else
	{
		CmdLnkdLstEnd->Next = e;
		e->Previous = CmdLnkdLstEnd;
		e->Next = 0;
		CmdLnkdLstEnd = e;
	}
}

void CmdLstRmTailElement()
{
	if (CmdLnkdLstEnd != CmdLnkdLstEntry)
	{
		CmdLnkdLstEnd = CmdLnkdLstEnd->Previous;
		free(CmdLnkdLstEnd->Next);
		CmdLnkdLstEnd->Next = 0;
	}
	else
	{
		free(CmdLnkdLstEnd);
		CmdLnkdLstEnd = CmdLnkdLstEntry = 0;
	}
}

void CmdLstRmElementByID(long id)
{
	
}

CmdLnkdLstElement* CmdLstSelectElementByID(long id)
{
	CmdLnkdLstElement* e = CmdLnkdLstEntry;
	while (e != 0)
	{
		if (e->Command.ID = id)
			break;
		e = e->Next;
	}
	return e;
}

void AddCommand(long id, char* name, void(*command)())
{
	CmdLnkdLstElement* e = (CmdLnkdLstElement*)malloc(sizeof(CmdLnkdLstElement));
	e->Command.ID = id;
	strcpy(e->Command.Name, name);
	e->Command.Execute = command;
	CmdLstAppendElement(e);
}

void ExecCommand(CmdLnkdLstElement* e)
{
	e->Command.Execute();
}

void ExecCommandByID(long id)
{
	CmdLnkdLstElement* e = CmdLstSelectElementByID(id);
	if (e != 0)
	{
		ExecCommand(e);
	}
}

CmdLnkdLstElement* CmdLstGetEntry()
{
	return CmdLnkdLstEntry;
}

CmdLnkdLstElement* CmdLstGetEnd()
{
	return CmdLnkdLstEnd;
}
