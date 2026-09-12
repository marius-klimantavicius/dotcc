/* Generated typed forwarding code. No default or success callbacks. */
#include "msquic_host_internal.h"

void* CxPlatAlloc(size_t ByteCount, uint32_t Tag)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatAlloc(Host->Context, ByteCount, Tag);
}

void* CxPlatAllocUninitialized(size_t ByteCount, uint32_t Tag)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatAllocUninitialized(Host->Context, ByteCount, Tag);
}

void CxPlatConvertFromMappedV6(const QUIC_ADDR* InAddr, QUIC_ADDR* OutAddr)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatConvertFromMappedV6(Host->Context, InAddr, OutAddr);
}

void CxPlatConvertToMappedV6(const QUIC_ADDR* InAddr, QUIC_ADDR* OutAddr)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatConvertToMappedV6(Host->Context, InAddr, OutAddr);
}

CXPLAT_THREAD_ID CxPlatCurThreadID(void)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatCurThreadID(Host->Context);
}

unsigned int CxPlatDataPathGetLocalAddressForRemote(const QUIC_ADDR* RemoteAddress, QUIC_ADDR* LocalAddress)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatDataPathGetLocalAddressForRemote(Host->Context, RemoteAddress, LocalAddress);
}

unsigned int CxPlatDataPathGetLocalAddresses(CXPLAT_DATAPATH* Datapath, CXPLAT_ADAPTER_ADDRESS** Addresses, uint32_t* AddressesCount)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatDataPathGetLocalAddresses(Host->Context, Datapath, Addresses, AddressesCount);
}

CXPLAT_DATAPATH_FEATURES CxPlatDataPathGetSupportedFeatures(CXPLAT_DATAPATH* Datapath, CXPLAT_SOCKET_FLAGS SocketFlags)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatDataPathGetSupportedFeatures(Host->Context, Datapath, SocketFlags);
}

unsigned int CxPlatDataPathInitialize(uint32_t ClientRecvContextLength, const CXPLAT_UDP_DATAPATH_CALLBACKS* UdpCallbacks, const CXPLAT_TCP_DATAPATH_CALLBACKS* TcpCallbacks, CXPLAT_WORKER_POOL* WorkerPool, CXPLAT_DATAPATH_INIT_CONFIG* InitConfig, CXPLAT_DATAPATH** NewDatapath)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatDataPathInitialize(Host->Context, ClientRecvContextLength, UdpCallbacks, TcpCallbacks, WorkerPool, InitConfig, NewDatapath);
}

BOOLEAN CxPlatDataPathIsPaddingPreferred(CXPLAT_DATAPATH* Datapath, CXPLAT_SEND_DATA* SendData)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatDataPathIsPaddingPreferred(Host->Context, Datapath, SendData);
}

unsigned int CxPlatDataPathResolveAddress(CXPLAT_DATAPATH* Datapath, const char* HostName, QUIC_ADDR* Address)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatDataPathResolveAddress(Host->Context, Datapath, HostName, Address);
}

void CxPlatDataPathRssConfigFree(CXPLAT_RSS_CONFIG* RssConfig)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatDataPathRssConfigFree(Host->Context, RssConfig);
}

unsigned int CxPlatDataPathRssConfigGet(uint32_t InterfaceIndex, CXPLAT_RSS_CONFIG** RssConfig)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatDataPathRssConfigGet(Host->Context, InterfaceIndex, RssConfig);
}

void CxPlatDataPathUninitialize(CXPLAT_DATAPATH* Datapath)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatDataPathUninitialize(Host->Context, Datapath);
}

void CxPlatDataPathUpdatePollingIdleTimeout(CXPLAT_DATAPATH* Datapath, uint32_t PollingIdleTimeoutUs)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatDataPathUpdatePollingIdleTimeout(Host->Context, Datapath, PollingIdleTimeoutUs);
}

unsigned int CxPlatDecrypt(CXPLAT_KEY* Key, const uint8_t* const Iv, uint16_t AuthDataLength, const uint8_t* const AuthData, uint16_t BufferLength, uint8_t* Buffer)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatDecrypt(Host->Context, Key, Iv, AuthDataLength, AuthData, BufferLength, Buffer);
}

unsigned int CxPlatEncrypt(CXPLAT_KEY* Key, const uint8_t* const Iv, uint16_t AuthDataLength, const uint8_t* const AuthData, uint16_t BufferLength, uint8_t* Buffer)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatEncrypt(Host->Context, Key, Iv, AuthDataLength, AuthData, BufferLength, Buffer);
}

void CxPlatEventInitialize(CXPLAT_EVENT* Event, BOOLEAN ManualReset, BOOLEAN InitialState)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatEventInitialize(Host->Context, Event, ManualReset, InitialState);
}

void CxPlatEventQCleanup(CXPLAT_EVENTQ* Queue)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatEventQCleanup(Host->Context, Queue);
}

uint32_t CxPlatEventQDequeue(CXPLAT_EVENTQ* Queue, CXPLAT_CQE* Events, uint32_t Count, uint32_t WaitTime)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatEventQDequeue(Host->Context, Queue, Events, Count, WaitTime);
}

BOOLEAN CxPlatEventQEnqueue(CXPLAT_EVENTQ* Queue, CXPLAT_SQE* Sqe)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatEventQEnqueue(Host->Context, Queue, Sqe);
}

BOOLEAN CxPlatEventQInitialize(CXPLAT_EVENTQ* Queue)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatEventQInitialize(Host->Context, Queue);
}

void CxPlatEventQReturn(CXPLAT_EVENTQ* Queue, uint32_t Count)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatEventQReturn(Host->Context, Queue, Count);
}

void CxPlatFree(void* Mem, uint32_t Tag)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatFree(Host->Context, Mem, Tag);
}

void CxPlatGetAbsoluteTime(unsigned long DeltaMs, struct timespec *Time)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatGetAbsoluteTime(Host->Context, DeltaMs, Time);
}

uint64_t CxPlatGetTimerResolution(void)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatGetTimerResolution(Host->Context);
}

unsigned int CxPlatHashCompute(CXPLAT_HASH* Hash, const uint8_t* const Input, uint32_t InputLength, uint32_t OutputLength, uint8_t* const Output)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatHashCompute(Host->Context, Hash, Input, InputLength, OutputLength, Output);
}

unsigned int CxPlatHashCreate(CXPLAT_HASH_TYPE HashType, const uint8_t* const Salt, uint32_t SaltLength, CXPLAT_HASH** Hash)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatHashCreate(Host->Context, HashType, Salt, SaltLength, Hash);
}

void CxPlatHashFree(CXPLAT_HASH* Hash)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatHashFree(Host->Context, Hash);
}

unsigned int CxPlatHpComputeMask(CXPLAT_HP_KEY* Key, uint8_t BatchSize, const uint8_t* const Cipher, uint8_t* Mask)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatHpComputeMask(Host->Context, Key, BatchSize, Cipher, Mask);
}

unsigned int CxPlatHpKeyCreate(CXPLAT_AEAD_TYPE AeadType, const uint8_t* const RawKey, CXPLAT_HP_KEY** Key)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatHpKeyCreate(Host->Context, AeadType, RawKey, Key);
}

void CxPlatHpKeyFree(CXPLAT_HP_KEY* Key)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatHpKeyFree(Host->Context, Key);
}

unsigned int CxPlatInitialize(void)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatInitialize(Host->Context);
}

void CxPlatInternalEventReset(CXPLAT_EVENT* Event)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatInternalEventReset(Host->Context, Event);
}

void CxPlatInternalEventSet(CXPLAT_EVENT* Event)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatInternalEventSet(Host->Context, Event);
}

void CxPlatInternalEventUninitialize(CXPLAT_EVENT* Event)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatInternalEventUninitialize(Host->Context, Event);
}

void CxPlatInternalEventWaitForever(CXPLAT_EVENT* Event)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatInternalEventWaitForever(Host->Context, Event);
}

BOOLEAN CxPlatInternalEventWaitWithTimeout(CXPLAT_EVENT* Event, uint32_t TimeoutMs)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatInternalEventWaitWithTimeout(Host->Context, Event, TimeoutMs);
}

unsigned int CxPlatKbKdfDerive(const uint8_t* Secret, uint32_t SecretLength, const char* Label, const uint8_t* Context, uint32_t ContextLength, uint32_t OutputLength, uint8_t* Output)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatKbKdfDerive(Host->Context, Secret, SecretLength, Label, Context, ContextLength, OutputLength, Output);
}

unsigned int CxPlatKeyCreate(CXPLAT_AEAD_TYPE AeadType, const uint8_t* const RawKey, CXPLAT_KEY** Key)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatKeyCreate(Host->Context, AeadType, RawKey, Key);
}

void CxPlatKeyFree(CXPLAT_KEY* Key)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatKeyFree(Host->Context, Key);
}

void CxPlatLockAcquire(CXPLAT_LOCK* Lock)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatLockAcquire(Host->Context, Lock);
}

void CxPlatLockInitialize(CXPLAT_LOCK* Lock)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatLockInitialize(Host->Context, Lock);
}

void CxPlatLockRelease(CXPLAT_LOCK* Lock)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatLockRelease(Host->Context, Lock);
}

void CxPlatLockUninitialize(CXPLAT_LOCK* Lock)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatLockUninitialize(Host->Context, Lock);
}

void CxPlatLogAssert(const char* File, int Line, const char* Expr)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatLogAssert(Host->Context, File, Line, Expr);
}

uint32_t CxPlatProcCurrentNumber(void)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatProcCurrentNumber(Host->Context);
}

unsigned int CxPlatRandom(uint32_t BufferLen, void* Buffer)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatRandom(Host->Context, BufferLen, Buffer);
}

void CxPlatRecvDataReturn(CXPLAT_RECV_DATA* RecvDataChain)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatRecvDataReturn(Host->Context, RecvDataChain);
}

unsigned int CxPlatResolveRoute(CXPLAT_SOCKET* Socket, CXPLAT_ROUTE* Route, uint8_t PathId, void* Context, CXPLAT_ROUTE_RESOLUTION_CALLBACK_HANDLER Callback)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatResolveRoute(Host->Context, Socket, Route, PathId, Context, Callback);
}

void CxPlatResolveRouteComplete(void* Context, CXPLAT_ROUTE* Route, const uint8_t* PhysicalAddress, uint8_t PathId)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatResolveRouteComplete(Host->Context, Context, Route, PhysicalAddress, PathId);
}

void CxPlatRwLockAcquireExclusive(CXPLAT_RW_LOCK* Lock)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatRwLockAcquireExclusive(Host->Context, Lock);
}

void CxPlatRwLockAcquireShared(CXPLAT_RW_LOCK* Lock)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatRwLockAcquireShared(Host->Context, Lock);
}

void CxPlatRwLockInitialize(CXPLAT_RW_LOCK* Lock)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatRwLockInitialize(Host->Context, Lock);
}

void CxPlatRwLockReleaseExclusive(CXPLAT_RW_LOCK* Lock)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatRwLockReleaseExclusive(Host->Context, Lock);
}

void CxPlatRwLockReleaseShared(CXPLAT_RW_LOCK* Lock)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatRwLockReleaseShared(Host->Context, Lock);
}

void CxPlatRwLockUninitialize(CXPLAT_RW_LOCK* Lock)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatRwLockUninitialize(Host->Context, Lock);
}

void CxPlatSchedulerYield(void)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatSchedulerYield(Host->Context);
}

CXPLAT_SEND_DATA* CxPlatSendDataAlloc(CXPLAT_SOCKET* Socket, CXPLAT_SEND_CONFIG* Config)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatSendDataAlloc(Host->Context, Socket, Config);
}

QUIC_BUFFER* CxPlatSendDataAllocBuffer(CXPLAT_SEND_DATA* SendData, uint16_t MaxBufferLength)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatSendDataAllocBuffer(Host->Context, SendData, MaxBufferLength);
}

void CxPlatSendDataFree(CXPLAT_SEND_DATA* SendData)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatSendDataFree(Host->Context, SendData);
}

void CxPlatSendDataFreeBuffer(CXPLAT_SEND_DATA* SendData, QUIC_BUFFER* Buffer)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatSendDataFreeBuffer(Host->Context, SendData, Buffer);
}

BOOLEAN CxPlatSendDataIsFull(CXPLAT_SEND_DATA* SendData)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatSendDataIsFull(Host->Context, SendData);
}

void CxPlatSleep(uint32_t DurationMs)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatSleep(Host->Context, DurationMs);
}

unsigned int CxPlatSocketCreateUdp(CXPLAT_DATAPATH* Datapath, const CXPLAT_UDP_CONFIG* Config, CXPLAT_SOCKET** Socket)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatSocketCreateUdp(Host->Context, Datapath, Config, Socket);
}

void CxPlatSocketDelete(CXPLAT_SOCKET* Socket)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatSocketDelete(Host->Context, Socket);
}

void CxPlatSocketGetLocalAddress(CXPLAT_SOCKET* Socket, QUIC_ADDR* Address)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatSocketGetLocalAddress(Host->Context, Socket, Address);
}

uint16_t CxPlatSocketGetLocalMtu(CXPLAT_SOCKET* Socket, CXPLAT_ROUTE* Route)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatSocketGetLocalMtu(Host->Context, Socket, Route);
}

BOOLEAN CxPlatSocketGetQtipEnabled(CXPLAT_SOCKET* Socket)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatSocketGetQtipEnabled(Host->Context, Socket);
}

void CxPlatSocketGetRemoteAddress(CXPLAT_SOCKET* Socket, QUIC_ADDR* Address)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatSocketGetRemoteAddress(Host->Context, Socket, Address);
}

void CxPlatSocketSend(CXPLAT_SOCKET* Socket, const CXPLAT_ROUTE* Route, CXPLAT_SEND_DATA* SendData)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatSocketSend(Host->Context, Socket, Route, SendData);
}

unsigned int CxPlatSocketUpdateQeo(CXPLAT_SOCKET* Socket, const CXPLAT_QEO_CONNECTION* Offloads, uint32_t OffloadCount)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatSocketUpdateQeo(Host->Context, Socket, Offloads, OffloadCount);
}

void CxPlatSqeCleanup(CXPLAT_EVENTQ* Queue, CXPLAT_SQE* Sqe)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatSqeCleanup(Host->Context, Queue, Sqe);
}

BOOLEAN CxPlatSqeInitialize(CXPLAT_EVENTQ* Queue, CXPLAT_EVENT_COMPLETION_HANDLER Completion, CXPLAT_SQE* Sqe)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatSqeInitialize(Host->Context, Queue, Completion, Sqe);
}

void CxPlatStorageClose(CXPLAT_STORAGE* Storage)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatStorageClose(Host->Context, Storage);
}

unsigned int CxPlatStorageOpen(const char * Path, CXPLAT_STORAGE_CHANGE_CALLBACK_HANDLER Callback, void* CallbackContext, CXPLAT_STORAGE_OPEN_FLAGS Flags, CXPLAT_STORAGE** NewStorage)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatStorageOpen(Host->Context, Path, Callback, CallbackContext, Flags, NewStorage);
}

unsigned int CxPlatStorageReadValue(CXPLAT_STORAGE* Storage, const char * Name, uint8_t * Buffer, uint32_t * BufferLength)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatStorageReadValue(Host->Context, Storage, Name, Buffer, BufferLength);
}

unsigned int CxPlatThreadCreate(CXPLAT_THREAD_CONFIG* Config, CXPLAT_THREAD* Thread)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatThreadCreate(Host->Context, Config, Thread);
}

void CxPlatThreadDelete(CXPLAT_THREAD* Thread)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatThreadDelete(Host->Context, Thread);
}

void CxPlatThreadWait(CXPLAT_THREAD* Thread)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatThreadWait(Host->Context, Thread);
}

int64_t CxPlatTimeEpochMs64(void)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatTimeEpochMs64(Host->Context);
}

uint64_t CxPlatTimeUs64(void)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatTimeUs64(Host->Context);
}

unsigned int CxPlatTlsExportKeyingMaterial(CXPLAT_TLS* TlsContext, const char* Label, const uint8_t* Context, uint32_t ContextLength, uint8_t* Output, uint32_t OutputLength)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatTlsExportKeyingMaterial(Host->Context, TlsContext, Label, Context, ContextLength, Output, OutputLength);
}

QUIC_TLS_PROVIDER CxPlatTlsGetProvider(void)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatTlsGetProvider(Host->Context);
}

unsigned int CxPlatTlsInitialize(const CXPLAT_TLS_CONFIG* Config, CXPLAT_TLS_PROCESS_STATE* State, CXPLAT_TLS** NewTlsContext)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatTlsInitialize(Host->Context, Config, State, NewTlsContext);
}

unsigned int CxPlatTlsParamGet(CXPLAT_TLS* TlsContext, uint32_t Param, uint32_t* BufferLength, void* Buffer)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatTlsParamGet(Host->Context, TlsContext, Param, BufferLength, Buffer);
}

unsigned int CxPlatTlsParamSet(CXPLAT_TLS* TlsContext, uint32_t Param, uint32_t BufferLength, const void* Buffer)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatTlsParamSet(Host->Context, TlsContext, Param, BufferLength, Buffer);
}

CXPLAT_TLS_RESULT_FLAGS CxPlatTlsProcessData(CXPLAT_TLS* TlsContext, CXPLAT_TLS_DATA_TYPE DataType, const uint8_t * Buffer, uint32_t * BufferLength, CXPLAT_TLS_PROCESS_STATE* State)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatTlsProcessData(Host->Context, TlsContext, DataType, Buffer, BufferLength, State);
}

unsigned int CxPlatTlsSecConfigCreate(const QUIC_CREDENTIAL_CONFIG* CredConfig, CXPLAT_TLS_CREDENTIAL_FLAGS TlsCredFlags, const CXPLAT_TLS_CALLBACKS* TlsCallbacks, void* Context, CXPLAT_SEC_CONFIG_CREATE_COMPLETE_HANDLER CompletionHandler)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatTlsSecConfigCreate(Host->Context, CredConfig, TlsCredFlags, TlsCallbacks, Context, CompletionHandler);
}

void CxPlatTlsSecConfigDelete(CXPLAT_SEC_CONFIG* SecurityConfig)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatTlsSecConfigDelete(Host->Context, SecurityConfig);
}

unsigned int CxPlatTlsSecConfigSetTicketKeys(CXPLAT_SEC_CONFIG* SecurityConfig, QUIC_TICKET_KEY_CONFIG* KeyConfig, uint8_t KeyCount)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->CxPlatTlsSecConfigSetTicketKeys(Host->Context, SecurityConfig, KeyConfig, KeyCount);
}

void CxPlatTlsUninitialize(CXPLAT_TLS* TlsContext)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatTlsUninitialize(Host->Context, TlsContext);
}

void CxPlatTlsUpdateHkdfLabels(CXPLAT_TLS* TlsContext, const QUIC_HKDF_LABELS* const Labels)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatTlsUpdateHkdfLabels(Host->Context, TlsContext, Labels);
}

void CxPlatUninitialize(void)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatUninitialize(Host->Context);
}

void CxPlatUpdateRoute(CXPLAT_ROUTE* DstRoute, CXPLAT_ROUTE* SrcRoute)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->CxPlatUpdateRoute(Host->Context, DstRoute, SrcRoute);
}

BOOLEAN QuicTlsPopulateOffloadKeys(CXPLAT_TLS* TlsContext, const QUIC_PACKET_KEY* const PacketKey, const char* const SecretName, CXPLAT_QEO_CONNECTION* Offload)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    return Host->QuicTlsPopulateOffloadKeys(Host->Context, TlsContext, PacketKey, SecretName, Offload);
}

void quic_bugcheck(const char* File, int Line, const char* Expr)
{
    const MSQUIC_HOST_TABLE* Host = MsQuicHostGet();
    Host->quic_bugcheck(Host->Context, File, Line, Expr);
    abort(); /* A returning fatal-error callback cannot resume C execution. */
}
