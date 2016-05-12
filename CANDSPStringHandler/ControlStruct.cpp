#include "ControlStruct.h"
#include <stdlib.h>
#include <string.h>

CtrlDtLnkdLstElement* LnkdLstEntry = 0;
CtrlDtLnkdLstElement* LnkdLstEnd = 0;

void LstAppendElement(CtrlDtLnkdLstElement* e)
{
	if (LnkdLstEntry == 0) 
	{
		LnkdLstEnd = LnkdLstEntry = e;
		e->Previous = e->Next = 0;
	}
	else
	{
		LnkdLstEnd->Next = e;
		e->Previous = LnkdLstEnd;
		e->Next = 0;
		LnkdLstEnd = e;
	}
}

void LstRmTailElement()
{
	if (LnkdLstEnd != 0)
	{
		LnkdLstEnd = LnkdLstEnd->Previous;
		free(LnkdLstEnd->Next);
		LnkdLstEnd->Next = 0;
	}
}

void LstRmElementByID(long id)
{
	CtrlDtLnkdLstElement* p = SelectLstElementByID(id);
	if (p != 0)
	{
		if (p->Previous != 0)
		{
			p->Previous->Next = p->Next;
		}
		else
		{
			LnkdLstEntry = p->Next;
		}
		if (p->Next != 0)
		{
			p->Next->Previous = p->Previous;
		}
		else
		{
			LnkdLstEnd = p->Previous;
		}
		free(p);
	}
}

CtrlDtLnkdLstElement* SelectLstElementByID(long id)
{
	CtrlDtLnkdLstElement* p = LnkdLstEntry;
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
	LstAppendElement(p);
}

CtrlDtLnkdLstElement* GetLnkdLstEntry()
{
	return LnkdLstEntry;
}

CtrlDtLnkdLstElement* GetLnkdLstEnd()
{
	return LnkdLstEnd;
}