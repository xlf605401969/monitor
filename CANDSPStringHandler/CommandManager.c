#include "CommandManager.h"
#include <stdlib.h>
#include <string.h>

CmdLnkdLstElement* CmdLnkdLstEntry = 0;
CmdLnkdLstElement* CmdLnkdLstEnd = 0;

void CmdLstAppend(CmdLnkdLstElement* e)
{
	if (CmdLnkdLstEntry == 0)
	{
		CmdLnkdLstEntry = CmdLnkdLstEnd = e;
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

void CmdLstRmTail()
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

void CmdLstRmByID(long id)
{
	
}

CmdLnkdLstElement* CmdLstSelectByID(long id)
{
	CmdLnkdLstElement* e = CmdLnkdLstEntry;
	while (e != 0)
	{
		if (e->Command.ID == id)
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
	CmdLstAppend(e);
}

void ExecCommand(CmdLnkdLstElement* e)
{
	e->Command.Execute();
}

void ExecCommandByID(long id)
{
	CmdLnkdLstElement* e = CmdLstSelectByID(id);
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

void InitCommandManager()
{
	CmdLnkdLstEntry = 0;
	CmdLnkdLstEnd = 0;
}
