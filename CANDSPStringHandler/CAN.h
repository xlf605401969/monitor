#ifndef _CAN_
#define _CAN_

#include "CANDriver.h"
#include "CANManager.h"
#include "CANQueue.h"
#include "CANString.h"
#include "CommandManager.h"
#include "ControlStruct.h"
#include "TimingTaskScheduler.h"

void inline InitCANAll()
{
#if defined(DSP)
	InitCANDriver();
#endif
	InitCANQueue();
	InitControlStruct();
	InitCommandManager();
	InitTaskScheduler();
	InitCANManager();
}

#endif
