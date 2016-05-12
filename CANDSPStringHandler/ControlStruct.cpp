#include "ControlStruct.h"
#include <stdlib.h>
#include <string.h>

CtrlDtLnkdLstElement* CtrlDtLnkdLstEntry = 0;
CtrlDtLnkdLstElement* CtrlDtLnkdLstEnd = 0;

void CtrlDtLstAppend(CtrlDtLnkdLstElement* e)
{
	if (CtrlDtLnkdLstEntry == 0) 
	{
		CtrlDtLnkdLstEnd = CtrlDtLnkdLstEntry = e;
		e->Previous = e->Next = 0;
	}
	else
	{
		CtrlDtLnkdLstEnd->Next = e;
		e->Previous = CtrlDtLnkdLstEnd;
		e->Next = 0;
		CtrlDtLnkdLstEnd = e;
	}
}

void CtrlDtLstRmTail()
{
	if (CtrlDtLnkdLstEnd != CtrlDtLnkdLstEntry)
	{
		CtrlDtLnkdLstEnd = CtrlDtLnkdLstEnd->Previous;
		free(CtrlDtLnkdLstEnd->Next);
		CtrlDtLnkdLstEnd->Next = 0;
	}
	else
	{
		free(CtrlDtLnkdLstEnd);
		CtrlDtLnkdLstEnd = CtrlDtLnkdLstEntry = 0;
	}

}

void CtrlDtLstRmByID(long id)
{
	CtrlDtLnkdLstElement* p = CtrlDtLstSelectByID(id);
	if (p != 0)
	{
		if (p->Previous != 0)
		{
			p->Previous->Next = p->Next;
		}
		else
		{
			CtrlDtLnkdLstEntry = p->Next;
		}
		if (p->Next != 0)
		{
			p->Next->Previous = p->Previous;
		}
		else
		{
			CtrlDtLnkdLstEnd = p->Previous;
		}
		free(p);
	}
}

CtrlDtLnkdLstElement* CtrlDtLstSelectByID(long id)
{
	CtrlDtLnkdLstElement* p = CtrlDtLnkdLstEntry;
	while (p != 0)
	{
		if (p->Data.ID == id)
			break;
		p = p->Next;
	}
	return p;
}

void AddCtrlData(long id, ControlDataType type, long isEditable, char* name, void* address)
{
	CtrlDtLnkdLstElement* p = (CtrlDtLnkdLstElement*)malloc(sizeof(CtrlDtLnkdLstElement));
	p->Data.ID = id;
	p->Data.Type = type;
	p->Data.IsEditable = isEditable;
	p->Data.Address = address;
	strcpy(p->Data.Name, name);
	CtrlDtLstAppend(p);
}

CtrlDtLnkdLstElement* CtrlDtLnkdLstGetEntry()
{
	return CtrlDtLnkdLstEntry;
}

CtrlDtLnkdLstElement* CtrlDtLnkdLstGetEnd()
{
	return CtrlDtLnkdLstEnd;
}