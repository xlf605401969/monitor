#include "CANDriver.h"
#include "CANQueue.h"

#define WINDOWS

#if defined(DSP)
#include "DSP28x_Project.h"
#endif

long IsCANBusy = 0;
long RecvEOFFlag = 0;

#if defined(DSP)
struct ECAN_REGS ECanaShadow;

void InitCANDriver()
{
	EALLOW;
	PieVectTable.ECAN0INTA = &CANRX_SERVER;
	PieVectTable.ECAN1INTA = &CANTX_SERVER;

	EnableInterrupts();
	PieCtrlRegs.PIEIER9.bit.INTx5 = 1;
	PieCtrlRegs.PIEIER9.bit.INTx6 = 1;

	IER |= M_INT9;
	EDIS;
	ERTM;

	EALLOW;
	ECanaMboxes.MBOX0.MDL.all = 0;
	ECanaMboxes.MBOX0.MDH.all = 0;
	ECanaMboxes.MBOX1.MDL.all = 0;
	ECanaMboxes.MBOX1.MDH.all = 0;
	//配置邮箱MBOX0,MBOX15的信息标识符寄存器(MSGID,11位标识符)
	ECanaMboxes.MBOX0.MSGID.bit.IDE = 0;//标准帧格式
	ECanaMboxes.MBOX0.MSGID.bit.AME = 0;//设置过滤
	ECanaMboxes.MBOX0.MSGID.bit.STDMSGID = 0x100; //邮箱ID
	ECanaLAMRegs.LAM0.bit.LAMI = 0;
	ECanaLAMRegs.LAM0.bit.LAM_H = 0x0000; //0000000001100;//仅接受0x200-0x203;
	ECanaLAMRegs.LAM0.bit.LAM_L = 0xFFFF;


	ECanaMboxes.MBOX1.MSGID.bit.IDE = 0;//标准帧格式
	ECanaMboxes.MBOX1.MSGID.bit.STDMSGID = 0x101;//邮箱ID

	ECanaShadow.CANMD.all = ECanaRegs.CANMD.all;
	ECanaShadow.CANMD.bit.MD0 = 1;      //邮箱0为接收
	ECanaShadow.CANMD.bit.MD1 = 0;      //邮箱1为发送
	ECanaRegs.CANMD.all = ECanaShadow.CANMD.all;

	//配置邮涞凝据长度为8个字节
	ECanaMboxes.MBOX0.MSGCTRL.bit.DLC = 8;//数据长度
	ECanaMboxes.MBOX1.MSGCTRL.bit.DLC = 8;
	ECanaMboxes.MBOX0.MSGCTRL.bit.RTR = 0;//无远程帧请求
	ECanaMboxes.MBOX1.MSGCTRL.bit.RTR = 0;

		//使能邮箱
	ECanaShadow.CANME.all = ECanaRegs.CANME.all;
	ECanaShadow.CANME.bit.ME0 = 1;      //使能0号邮箱
	ECanaShadow.CANME.bit.ME1 = 1;      //使能1号邮箱
	ECanaRegs.CANME.all = ECanaShadow.CANME.all;

	//configure CAN interrupt
	ECanaShadow.CANMIM.all = ECanaRegs.CANMIM.all;
	ECanaShadow.CANMIM.bit.MIM0 = 1;	//mailbox0 interrupt enable
	ECanaShadow.CANMIM.bit.MIM1 = 1;	//mailbox1 interrupt enable
	ECanaRegs.CANMIM.all =  ECanaShadow.CANMIM.all;

	ECanaShadow.CANMIL.all = ECanaRegs.CANMIL.all;
	ECanaShadow.CANMIL.bit.MIL0 = 0;	//select  ECAN0INT interrupt line
	ECanaShadow.CANMIL.bit.MIL1 = 1;	//select ECAN1INT interrupt line
	ECanaRegs.CANMIL.all = ECanaShadow.CANMIL.all;

	ECanaShadow.CANGIM.all = ECanaRegs.CANGIM.all;
	ECanaShadow.CANGIM.bit.AAIM = 1;	//Enable Abort acknowledge interrupt

	//Global interrupt for the interrupts TCOF,WIF,WUIF,BOIF,EPIF
	//RMLIF,AAIF,WLIF
	ECanaShadow.CANGIM.bit.GIL = 0;		//1---All global interrupts are mapped
										// to the ECAN1INT interrupt line
										//0---All global interrupts are mapped
										// to the ECAN0INT interrupt line
	ECanaShadow.CANGIM.bit.RMLIM = 1;	//receive message lost
	ECanaShadow.CANGIM.bit.I0EN = 1;	//interrupt 0 enable INT9.5
	ECanaShadow.CANGIM.bit.I1EN = 1;	//if GIL =1  INT9.6
	ECanaRegs.CANGIM.all = ECanaShadow.CANGIM.all;
	EDIS;
}

interrupt void CANRX_SERVER()
{
	struct ECAN_REGS ECanaShadow;
	Uint16 CANRX_MSGID;

	ECanaShadow.CANRMP.all = ECanaRegs.CANRMP.all;
	ECanaShadow.CANRMP.bit.RMP0 = 1;//clear  RMLIF0(接收邮箱溢出) by set RMP0
	ECanaRegs.CANRMP.all = ECanaShadow.CANRMP.all;

	CANRX_MSGID = ECanaMboxes.MBOX0.MSGID.bit.STDMSGID;

	if (CANRX_MSGID == 0x100)
	{
		char data[8];
		data[0] = ECanaMboxes.MBOX0.MDL.byte.BYTE0;
		data[1] = ECanaMboxes.MBOX0.MDL.byte.BYTE1;
		data[2] = ECanaMboxes.MBOX0.MDL.byte.BYTE2;
		data[3] = ECanaMboxes.MBOX0.MDL.byte.BYTE3;
		data[4] = ECanaMboxes.MBOX0.MDH.byte.BYTE4;
		data[5] = ECanaMboxes.MBOX0.MDH.byte.BYTE5;
		data[6] = ECanaMboxes.MBOX0.MDH.byte.BYTE6;
		data[7] = ECanaMboxes.MBOX0.MDH.byte.BYTE7;
		EnqueueCANData(data);
	}
	PieCtrlRegs.PIEACK.all=PIEACK_GROUP9;
}

interrupt void CANTX_SERVER()
{
	struct ECAN_REGS ECanaShadow;

	ECanaShadow.CANTA.all = 0;
	ECanaShadow.CANTA.bit.TA1 = 1;
	ECanaRegs.CANTA.all = ECanaShadow.CANTA.all;

	if (SendQueueLength())
	{
		char data[8];
		DequeueCANData(data);

		ECanaMboxes.MBOX1.MDL.byte.BYTE0 = data[0];
		ECanaMboxes.MBOX1.MDL.byte.BYTE1 = data[1];
		ECanaMboxes.MBOX1.MDL.byte.BYTE2 = data[2];
		ECanaMboxes.MBOX1.MDL.byte.BYTE3 = data[3];
		ECanaMboxes.MBOX1.MDH.byte.BYTE4 = data[4];
		ECanaMboxes.MBOX1.MDH.byte.BYTE5 = data[5];
		ECanaMboxes.MBOX1.MDH.byte.BYTE6 = data[6];
		ECanaMboxes.MBOX1.MDH.byte.BYTE7 = data[7];

		ECanaShadow.CANTRS.all = 0;
		ECanaShadow.CANTRS.bit.TRS1 = 1;
		ECanaRegs.CANTRS.all = ECanaShadow.CANTRS.all;
	}
	else
	{
		IsCANBusy = 0;
	}
	PieCtrlRegs.PIEACK.all=PIEACK_GROUP9;	// 第九组中断

}

void CAN_TRY_SEND()
{
	if (SendQueueLength())
	{
		char data[8];
		DequeueCANData(data);

		ECanaMboxes.MBOX1.MDL.byte.BYTE0 = data[0];
		ECanaMboxes.MBOX1.MDL.byte.BYTE1 = data[1];
		ECanaMboxes.MBOX1.MDL.byte.BYTE2 = data[2];
		ECanaMboxes.MBOX1.MDL.byte.BYTE3 = data[3];
		ECanaMboxes.MBOX1.MDH.byte.BYTE4 = data[4];
		ECanaMboxes.MBOX1.MDH.byte.BYTE5 = data[5];
		ECanaMboxes.MBOX1.MDH.byte.BYTE6 = data[6];
		ECanaMboxes.MBOX1.MDH.byte.BYTE7 = data[7];

		ECanaShadow.CANTRS.all = 0;
		ECanaShadow.CANTRS.bit.TRS1 = 1;
		ECanaRegs.CANTRS.all = ECanaShadow.CANTRS.all;
		IsCANBusy = 1;
	}

}

void EnqueueCANData(char* bytes)
{
	int i;
	for(i = 0; i < 8; i++)
	{
		if (bytes[i] == 0x04)
			break;
		if (bytes[i] == 0xff)
			RecvEOFFlag = 1;
		EnqueueRecv(bytes[i]);
	}
}

void DequeueCANData(char* bytes)
{
	int i;
	for (i = 0; i < 8 && SendQueueLength() > 0; i++)
	{
		bytes[i] = DequeueSend();
	}
	if (i < 8)
	{
		bytes[i] = 0x04;
	}
}
#endif

#if defined(WINDOWS)
void CAN_TRY_SEND()
{

}
#endif
