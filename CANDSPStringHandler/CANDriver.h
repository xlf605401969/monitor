#ifndef _CAN_DRIVER_
#define _CAN_DRIVER_


extern long IsCANBusy;
extern long RecvEOFFlag;

void InitCANDriver();

void CANRX_SERVER();

void CANTX_SERVER();

void CAN_TRY_SEND();

void EnqueueCANData(char* bytes);

void DequeueCANData(char* bytes);

#endif
