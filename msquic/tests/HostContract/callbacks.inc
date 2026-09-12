/* Generated test-only fail-fast callbacks; no provider service is simulated. */
#include "msquic_host.h"
#include <stdio.h>

static void* Fail_CxPlatAlloc(void* HostContext, size_t ByteCount, uint32_t Tag) { abort(); }
static void* Fail_CxPlatAllocUninitialized(void* HostContext, size_t ByteCount, uint32_t Tag) { abort(); }
static void Fail_CxPlatConvertFromMappedV6(void* HostContext, const QUIC_ADDR* InAddr, QUIC_ADDR* OutAddr) { abort(); }
static void Fail_CxPlatConvertToMappedV6(void* HostContext, const QUIC_ADDR* InAddr, QUIC_ADDR* OutAddr) { abort(); }
static CXPLAT_THREAD_ID Fail_CxPlatCurThreadID(void* HostContext) { abort(); }
static unsigned int Fail_CxPlatDataPathGetLocalAddressForRemote(void* HostContext, const QUIC_ADDR* RemoteAddress, QUIC_ADDR* LocalAddress) { abort(); }
static unsigned int Fail_CxPlatDataPathGetLocalAddresses(void* HostContext, CXPLAT_DATAPATH* Datapath, CXPLAT_ADAPTER_ADDRESS** Addresses, uint32_t* AddressesCount) { abort(); }
static CXPLAT_DATAPATH_FEATURES Fail_CxPlatDataPathGetSupportedFeatures(void* HostContext, CXPLAT_DATAPATH* Datapath, CXPLAT_SOCKET_FLAGS SocketFlags) { abort(); }
static unsigned int Fail_CxPlatDataPathInitialize(void* HostContext, uint32_t ClientRecvContextLength, const CXPLAT_UDP_DATAPATH_CALLBACKS* UdpCallbacks, const CXPLAT_TCP_DATAPATH_CALLBACKS* TcpCallbacks, CXPLAT_WORKER_POOL* WorkerPool, CXPLAT_DATAPATH_INIT_CONFIG* InitConfig, CXPLAT_DATAPATH** NewDatapath) { abort(); }
static BOOLEAN Fail_CxPlatDataPathIsPaddingPreferred(void* HostContext, CXPLAT_DATAPATH* Datapath, CXPLAT_SEND_DATA* SendData) { abort(); }
static unsigned int Fail_CxPlatDataPathResolveAddress(void* HostContext, CXPLAT_DATAPATH* Datapath, const char* HostName, QUIC_ADDR* Address) { abort(); }
static void Fail_CxPlatDataPathRssConfigFree(void* HostContext, CXPLAT_RSS_CONFIG* RssConfig) { abort(); }
static unsigned int Fail_CxPlatDataPathRssConfigGet(void* HostContext, uint32_t InterfaceIndex, CXPLAT_RSS_CONFIG** RssConfig) { abort(); }
static void Fail_CxPlatDataPathUninitialize(void* HostContext, CXPLAT_DATAPATH* Datapath) { abort(); }
static void Fail_CxPlatDataPathUpdatePollingIdleTimeout(void* HostContext, CXPLAT_DATAPATH* Datapath, uint32_t PollingIdleTimeoutUs) { abort(); }
static unsigned int Fail_CxPlatDecrypt(void* HostContext, CXPLAT_KEY* Key, const uint8_t* const Iv, uint16_t AuthDataLength, const uint8_t* const AuthData, uint16_t BufferLength, uint8_t* Buffer) { abort(); }
static unsigned int Fail_CxPlatEncrypt(void* HostContext, CXPLAT_KEY* Key, const uint8_t* const Iv, uint16_t AuthDataLength, const uint8_t* const AuthData, uint16_t BufferLength, uint8_t* Buffer) { abort(); }
static void Fail_CxPlatEventInitialize(void* HostContext, CXPLAT_EVENT* Event, BOOLEAN ManualReset, BOOLEAN InitialState) { abort(); }
static void Fail_CxPlatEventQCleanup(void* HostContext, CXPLAT_EVENTQ* Queue) { abort(); }
static uint32_t Fail_CxPlatEventQDequeue(void* HostContext, CXPLAT_EVENTQ* Queue, CXPLAT_CQE* Events, uint32_t Count, uint32_t WaitTime) { abort(); }
static BOOLEAN Fail_CxPlatEventQEnqueue(void* HostContext, CXPLAT_EVENTQ* Queue, CXPLAT_SQE* Sqe) { abort(); }
static BOOLEAN Fail_CxPlatEventQInitialize(void* HostContext, CXPLAT_EVENTQ* Queue) { abort(); }
static void Fail_CxPlatEventQReturn(void* HostContext, CXPLAT_EVENTQ* Queue, uint32_t Count) { abort(); }
static void Fail_CxPlatFree(void* HostContext, void* Mem, uint32_t Tag) { abort(); }
static void Fail_CxPlatGetAbsoluteTime(void* HostContext, unsigned long DeltaMs, struct timespec *Time) { abort(); }
static uint64_t Fail_CxPlatGetTimerResolution(void* HostContext) { abort(); }
static unsigned int Fail_CxPlatHashCompute(void* HostContext, CXPLAT_HASH* Hash, const uint8_t* const Input, uint32_t InputLength, uint32_t OutputLength, uint8_t* const Output) { abort(); }
static unsigned int Fail_CxPlatHashCreate(void* HostContext, CXPLAT_HASH_TYPE HashType, const uint8_t* const Salt, uint32_t SaltLength, CXPLAT_HASH** Hash) { abort(); }
static void Fail_CxPlatHashFree(void* HostContext, CXPLAT_HASH* Hash) { abort(); }
static unsigned int Fail_CxPlatHpComputeMask(void* HostContext, CXPLAT_HP_KEY* Key, uint8_t BatchSize, const uint8_t* const Cipher, uint8_t* Mask) { abort(); }
static unsigned int Fail_CxPlatHpKeyCreate(void* HostContext, CXPLAT_AEAD_TYPE AeadType, const uint8_t* const RawKey, CXPLAT_HP_KEY** Key) { abort(); }
static void Fail_CxPlatHpKeyFree(void* HostContext, CXPLAT_HP_KEY* Key) { abort(); }
static unsigned int Fail_CxPlatInitialize(void* HostContext) { abort(); }
static void Fail_CxPlatInternalEventReset(void* HostContext, CXPLAT_EVENT* Event) { abort(); }
static void Fail_CxPlatInternalEventSet(void* HostContext, CXPLAT_EVENT* Event) { abort(); }
static void Fail_CxPlatInternalEventUninitialize(void* HostContext, CXPLAT_EVENT* Event) { abort(); }
static void Fail_CxPlatInternalEventWaitForever(void* HostContext, CXPLAT_EVENT* Event) { abort(); }
static BOOLEAN Fail_CxPlatInternalEventWaitWithTimeout(void* HostContext, CXPLAT_EVENT* Event, uint32_t TimeoutMs) { abort(); }
static unsigned int Fail_CxPlatKbKdfDerive(void* HostContext, const uint8_t* Secret, uint32_t SecretLength, const char* Label, const uint8_t* Context, uint32_t ContextLength, uint32_t OutputLength, uint8_t* Output) { abort(); }
static unsigned int Fail_CxPlatKeyCreate(void* HostContext, CXPLAT_AEAD_TYPE AeadType, const uint8_t* const RawKey, CXPLAT_KEY** Key) { abort(); }
static void Fail_CxPlatKeyFree(void* HostContext, CXPLAT_KEY* Key) { abort(); }
static void Fail_CxPlatLockAcquire(void* HostContext, CXPLAT_LOCK* Lock) { abort(); }
static void Fail_CxPlatLockInitialize(void* HostContext, CXPLAT_LOCK* Lock) { abort(); }
static void Fail_CxPlatLockRelease(void* HostContext, CXPLAT_LOCK* Lock) { abort(); }
static void Fail_CxPlatLockUninitialize(void* HostContext, CXPLAT_LOCK* Lock) { abort(); }
static void Fail_CxPlatLogAssert(void* HostContext, const char* File, int Line, const char* Expr) { abort(); }
static uint32_t Fail_CxPlatProcCurrentNumber(void* HostContext) { abort(); }
static unsigned int Fail_CxPlatRandom(void* HostContext, uint32_t BufferLen, void* Buffer) { abort(); }
static void Fail_CxPlatRecvDataReturn(void* HostContext, CXPLAT_RECV_DATA* RecvDataChain) { abort(); }
static unsigned int Fail_CxPlatResolveRoute(void* HostContext, CXPLAT_SOCKET* Socket, CXPLAT_ROUTE* Route, uint8_t PathId, void* Context, CXPLAT_ROUTE_RESOLUTION_CALLBACK_HANDLER Callback) { abort(); }
static void Fail_CxPlatResolveRouteComplete(void* HostContext, void* Context, CXPLAT_ROUTE* Route, const uint8_t* PhysicalAddress, uint8_t PathId) { abort(); }
static void Fail_CxPlatRwLockAcquireExclusive(void* HostContext, CXPLAT_RW_LOCK* Lock) { abort(); }
static void Fail_CxPlatRwLockAcquireShared(void* HostContext, CXPLAT_RW_LOCK* Lock) { abort(); }
static void Fail_CxPlatRwLockInitialize(void* HostContext, CXPLAT_RW_LOCK* Lock) { abort(); }
static void Fail_CxPlatRwLockReleaseExclusive(void* HostContext, CXPLAT_RW_LOCK* Lock) { abort(); }
static void Fail_CxPlatRwLockReleaseShared(void* HostContext, CXPLAT_RW_LOCK* Lock) { abort(); }
static void Fail_CxPlatRwLockUninitialize(void* HostContext, CXPLAT_RW_LOCK* Lock) { abort(); }
static void Fail_CxPlatSchedulerYield(void* HostContext) { abort(); }
static CXPLAT_SEND_DATA* Fail_CxPlatSendDataAlloc(void* HostContext, CXPLAT_SOCKET* Socket, CXPLAT_SEND_CONFIG* Config) { abort(); }
static QUIC_BUFFER* Fail_CxPlatSendDataAllocBuffer(void* HostContext, CXPLAT_SEND_DATA* SendData, uint16_t MaxBufferLength) { abort(); }
static void Fail_CxPlatSendDataFree(void* HostContext, CXPLAT_SEND_DATA* SendData) { abort(); }
static void Fail_CxPlatSendDataFreeBuffer(void* HostContext, CXPLAT_SEND_DATA* SendData, QUIC_BUFFER* Buffer) { abort(); }
static BOOLEAN Fail_CxPlatSendDataIsFull(void* HostContext, CXPLAT_SEND_DATA* SendData) { abort(); }
static void Fail_CxPlatSleep(void* HostContext, uint32_t DurationMs) { abort(); }
static unsigned int Fail_CxPlatSocketCreateUdp(void* HostContext, CXPLAT_DATAPATH* Datapath, const CXPLAT_UDP_CONFIG* Config, CXPLAT_SOCKET** Socket) { abort(); }
static void Fail_CxPlatSocketDelete(void* HostContext, CXPLAT_SOCKET* Socket) { abort(); }
static void Fail_CxPlatSocketGetLocalAddress(void* HostContext, CXPLAT_SOCKET* Socket, QUIC_ADDR* Address) { abort(); }
static uint16_t Fail_CxPlatSocketGetLocalMtu(void* HostContext, CXPLAT_SOCKET* Socket, CXPLAT_ROUTE* Route) { abort(); }
static BOOLEAN Fail_CxPlatSocketGetQtipEnabled(void* HostContext, CXPLAT_SOCKET* Socket) { abort(); }
static void Fail_CxPlatSocketGetRemoteAddress(void* HostContext, CXPLAT_SOCKET* Socket, QUIC_ADDR* Address) { abort(); }
static void Fail_CxPlatSocketSend(void* HostContext, CXPLAT_SOCKET* Socket, const CXPLAT_ROUTE* Route, CXPLAT_SEND_DATA* SendData) { abort(); }
static unsigned int Fail_CxPlatSocketUpdateQeo(void* HostContext, CXPLAT_SOCKET* Socket, const CXPLAT_QEO_CONNECTION* Offloads, uint32_t OffloadCount) { abort(); }
static void Fail_CxPlatSqeCleanup(void* HostContext, CXPLAT_EVENTQ* Queue, CXPLAT_SQE* Sqe) { abort(); }
static BOOLEAN Fail_CxPlatSqeInitialize(void* HostContext, CXPLAT_EVENTQ* Queue, CXPLAT_EVENT_COMPLETION_HANDLER Completion, CXPLAT_SQE* Sqe) { abort(); }
static void Fail_CxPlatStorageClose(void* HostContext, CXPLAT_STORAGE* Storage) { abort(); }
static unsigned int Fail_CxPlatStorageOpen(void* HostContext, const char * Path, CXPLAT_STORAGE_CHANGE_CALLBACK_HANDLER Callback, void* CallbackContext, CXPLAT_STORAGE_OPEN_FLAGS Flags, CXPLAT_STORAGE** NewStorage) { abort(); }
static unsigned int Fail_CxPlatStorageReadValue(void* HostContext, CXPLAT_STORAGE* Storage, const char * Name, uint8_t * Buffer, uint32_t * BufferLength) { abort(); }
static void Fail_CxPlatSystemLoad(void* HostContext) { abort(); }
static void Fail_CxPlatSystemUnload(void* HostContext) { abort(); }
static unsigned int Fail_CxPlatThreadCreate(void* HostContext, CXPLAT_THREAD_CONFIG* Config, CXPLAT_THREAD* Thread) { abort(); }
static void Fail_CxPlatThreadDelete(void* HostContext, CXPLAT_THREAD* Thread) { abort(); }
static void Fail_CxPlatThreadWait(void* HostContext, CXPLAT_THREAD* Thread) { abort(); }
static int64_t Fail_CxPlatTimeEpochMs64(void* HostContext) { abort(); }
static uint64_t Fail_CxPlatTimeUs64(void* HostContext) { abort(); }
static unsigned int Fail_CxPlatTlsExportKeyingMaterial(void* HostContext, CXPLAT_TLS* TlsContext, const char* Label, const uint8_t* Context, uint32_t ContextLength, uint8_t* Output, uint32_t OutputLength) { abort(); }
static QUIC_TLS_PROVIDER Fail_CxPlatTlsGetProvider(void* HostContext) { abort(); }
static unsigned int Fail_CxPlatTlsInitialize(void* HostContext, const CXPLAT_TLS_CONFIG* Config, CXPLAT_TLS_PROCESS_STATE* State, CXPLAT_TLS** NewTlsContext) { abort(); }
static unsigned int Fail_CxPlatTlsParamGet(void* HostContext, CXPLAT_TLS* TlsContext, uint32_t Param, uint32_t* BufferLength, void* Buffer) { abort(); }
static unsigned int Fail_CxPlatTlsParamSet(void* HostContext, CXPLAT_TLS* TlsContext, uint32_t Param, uint32_t BufferLength, const void* Buffer) { abort(); }
static CXPLAT_TLS_RESULT_FLAGS Fail_CxPlatTlsProcessData(void* HostContext, CXPLAT_TLS* TlsContext, CXPLAT_TLS_DATA_TYPE DataType, const uint8_t * Buffer, uint32_t * BufferLength, CXPLAT_TLS_PROCESS_STATE* State) { abort(); }
static unsigned int Fail_CxPlatTlsSecConfigCreate(void* HostContext, const QUIC_CREDENTIAL_CONFIG* CredConfig, CXPLAT_TLS_CREDENTIAL_FLAGS TlsCredFlags, const CXPLAT_TLS_CALLBACKS* TlsCallbacks, void* Context, CXPLAT_SEC_CONFIG_CREATE_COMPLETE_HANDLER CompletionHandler) { abort(); }
static void Fail_CxPlatTlsSecConfigDelete(void* HostContext, CXPLAT_SEC_CONFIG* SecurityConfig) { abort(); }
static unsigned int Fail_CxPlatTlsSecConfigSetTicketKeys(void* HostContext, CXPLAT_SEC_CONFIG* SecurityConfig, QUIC_TICKET_KEY_CONFIG* KeyConfig, uint8_t KeyCount) { abort(); }
static void Fail_CxPlatTlsUninitialize(void* HostContext, CXPLAT_TLS* TlsContext) { abort(); }
static void Fail_CxPlatTlsUpdateHkdfLabels(void* HostContext, CXPLAT_TLS* TlsContext, const QUIC_HKDF_LABELS* const Labels) { abort(); }
static void Fail_CxPlatUninitialize(void* HostContext) { abort(); }
static void Fail_CxPlatUpdateRoute(void* HostContext, CXPLAT_ROUTE* DstRoute, CXPLAT_ROUTE* SrcRoute) { abort(); }
static BOOLEAN Fail_QuicTlsPopulateOffloadKeys(void* HostContext, CXPLAT_TLS* TlsContext, const QUIC_PACKET_KEY* const PacketKey, const char* const SecretName, CXPLAT_QEO_CONNECTION* Offload) { abort(); }
static void Fail_quic_bugcheck(void* HostContext, const char* File, int Line, const char* Expr) { abort(); }

static void Populate(MSQUIC_HOST_TABLE* Table) {
    memset(Table, 0, sizeof(*Table));
    Table->Size = sizeof(*Table); Table->Version = MSQUIC_HOST_VERSION;
    Table->ProcessorCount = 2; Table->TotalMemory = 4294967296ULL;
    Table->CxPlatAlloc = Fail_CxPlatAlloc;
    Table->CxPlatAllocUninitialized = Fail_CxPlatAllocUninitialized;
    Table->CxPlatConvertFromMappedV6 = Fail_CxPlatConvertFromMappedV6;
    Table->CxPlatConvertToMappedV6 = Fail_CxPlatConvertToMappedV6;
    Table->CxPlatCurThreadID = Fail_CxPlatCurThreadID;
    Table->CxPlatDataPathGetLocalAddressForRemote = Fail_CxPlatDataPathGetLocalAddressForRemote;
    Table->CxPlatDataPathGetLocalAddresses = Fail_CxPlatDataPathGetLocalAddresses;
    Table->CxPlatDataPathGetSupportedFeatures = Fail_CxPlatDataPathGetSupportedFeatures;
    Table->CxPlatDataPathInitialize = Fail_CxPlatDataPathInitialize;
    Table->CxPlatDataPathIsPaddingPreferred = Fail_CxPlatDataPathIsPaddingPreferred;
    Table->CxPlatDataPathResolveAddress = Fail_CxPlatDataPathResolveAddress;
    Table->CxPlatDataPathRssConfigFree = Fail_CxPlatDataPathRssConfigFree;
    Table->CxPlatDataPathRssConfigGet = Fail_CxPlatDataPathRssConfigGet;
    Table->CxPlatDataPathUninitialize = Fail_CxPlatDataPathUninitialize;
    Table->CxPlatDataPathUpdatePollingIdleTimeout = Fail_CxPlatDataPathUpdatePollingIdleTimeout;
    Table->CxPlatDecrypt = Fail_CxPlatDecrypt;
    Table->CxPlatEncrypt = Fail_CxPlatEncrypt;
    Table->CxPlatEventInitialize = Fail_CxPlatEventInitialize;
    Table->CxPlatEventQCleanup = Fail_CxPlatEventQCleanup;
    Table->CxPlatEventQDequeue = Fail_CxPlatEventQDequeue;
    Table->CxPlatEventQEnqueue = Fail_CxPlatEventQEnqueue;
    Table->CxPlatEventQInitialize = Fail_CxPlatEventQInitialize;
    Table->CxPlatEventQReturn = Fail_CxPlatEventQReturn;
    Table->CxPlatFree = Fail_CxPlatFree;
    Table->CxPlatGetAbsoluteTime = Fail_CxPlatGetAbsoluteTime;
    Table->CxPlatGetTimerResolution = Fail_CxPlatGetTimerResolution;
    Table->CxPlatHashCompute = Fail_CxPlatHashCompute;
    Table->CxPlatHashCreate = Fail_CxPlatHashCreate;
    Table->CxPlatHashFree = Fail_CxPlatHashFree;
    Table->CxPlatHpComputeMask = Fail_CxPlatHpComputeMask;
    Table->CxPlatHpKeyCreate = Fail_CxPlatHpKeyCreate;
    Table->CxPlatHpKeyFree = Fail_CxPlatHpKeyFree;
    Table->CxPlatInitialize = Fail_CxPlatInitialize;
    Table->CxPlatInternalEventReset = Fail_CxPlatInternalEventReset;
    Table->CxPlatInternalEventSet = Fail_CxPlatInternalEventSet;
    Table->CxPlatInternalEventUninitialize = Fail_CxPlatInternalEventUninitialize;
    Table->CxPlatInternalEventWaitForever = Fail_CxPlatInternalEventWaitForever;
    Table->CxPlatInternalEventWaitWithTimeout = Fail_CxPlatInternalEventWaitWithTimeout;
    Table->CxPlatKbKdfDerive = Fail_CxPlatKbKdfDerive;
    Table->CxPlatKeyCreate = Fail_CxPlatKeyCreate;
    Table->CxPlatKeyFree = Fail_CxPlatKeyFree;
    Table->CxPlatLockAcquire = Fail_CxPlatLockAcquire;
    Table->CxPlatLockInitialize = Fail_CxPlatLockInitialize;
    Table->CxPlatLockRelease = Fail_CxPlatLockRelease;
    Table->CxPlatLockUninitialize = Fail_CxPlatLockUninitialize;
    Table->CxPlatLogAssert = Fail_CxPlatLogAssert;
    Table->CxPlatProcCurrentNumber = Fail_CxPlatProcCurrentNumber;
    Table->CxPlatRandom = Fail_CxPlatRandom;
    Table->CxPlatRecvDataReturn = Fail_CxPlatRecvDataReturn;
    Table->CxPlatResolveRoute = Fail_CxPlatResolveRoute;
    Table->CxPlatResolveRouteComplete = Fail_CxPlatResolveRouteComplete;
    Table->CxPlatRwLockAcquireExclusive = Fail_CxPlatRwLockAcquireExclusive;
    Table->CxPlatRwLockAcquireShared = Fail_CxPlatRwLockAcquireShared;
    Table->CxPlatRwLockInitialize = Fail_CxPlatRwLockInitialize;
    Table->CxPlatRwLockReleaseExclusive = Fail_CxPlatRwLockReleaseExclusive;
    Table->CxPlatRwLockReleaseShared = Fail_CxPlatRwLockReleaseShared;
    Table->CxPlatRwLockUninitialize = Fail_CxPlatRwLockUninitialize;
    Table->CxPlatSchedulerYield = Fail_CxPlatSchedulerYield;
    Table->CxPlatSendDataAlloc = Fail_CxPlatSendDataAlloc;
    Table->CxPlatSendDataAllocBuffer = Fail_CxPlatSendDataAllocBuffer;
    Table->CxPlatSendDataFree = Fail_CxPlatSendDataFree;
    Table->CxPlatSendDataFreeBuffer = Fail_CxPlatSendDataFreeBuffer;
    Table->CxPlatSendDataIsFull = Fail_CxPlatSendDataIsFull;
    Table->CxPlatSleep = Fail_CxPlatSleep;
    Table->CxPlatSocketCreateUdp = Fail_CxPlatSocketCreateUdp;
    Table->CxPlatSocketDelete = Fail_CxPlatSocketDelete;
    Table->CxPlatSocketGetLocalAddress = Fail_CxPlatSocketGetLocalAddress;
    Table->CxPlatSocketGetLocalMtu = Fail_CxPlatSocketGetLocalMtu;
    Table->CxPlatSocketGetQtipEnabled = Fail_CxPlatSocketGetQtipEnabled;
    Table->CxPlatSocketGetRemoteAddress = Fail_CxPlatSocketGetRemoteAddress;
    Table->CxPlatSocketSend = Fail_CxPlatSocketSend;
    Table->CxPlatSocketUpdateQeo = Fail_CxPlatSocketUpdateQeo;
    Table->CxPlatSqeCleanup = Fail_CxPlatSqeCleanup;
    Table->CxPlatSqeInitialize = Fail_CxPlatSqeInitialize;
    Table->CxPlatStorageClose = Fail_CxPlatStorageClose;
    Table->CxPlatStorageOpen = Fail_CxPlatStorageOpen;
    Table->CxPlatStorageReadValue = Fail_CxPlatStorageReadValue;
    Table->CxPlatSystemLoad = Fail_CxPlatSystemLoad;
    Table->CxPlatSystemUnload = Fail_CxPlatSystemUnload;
    Table->CxPlatThreadCreate = Fail_CxPlatThreadCreate;
    Table->CxPlatThreadDelete = Fail_CxPlatThreadDelete;
    Table->CxPlatThreadWait = Fail_CxPlatThreadWait;
    Table->CxPlatTimeEpochMs64 = Fail_CxPlatTimeEpochMs64;
    Table->CxPlatTimeUs64 = Fail_CxPlatTimeUs64;
    Table->CxPlatTlsExportKeyingMaterial = Fail_CxPlatTlsExportKeyingMaterial;
    Table->CxPlatTlsGetProvider = Fail_CxPlatTlsGetProvider;
    Table->CxPlatTlsInitialize = Fail_CxPlatTlsInitialize;
    Table->CxPlatTlsParamGet = Fail_CxPlatTlsParamGet;
    Table->CxPlatTlsParamSet = Fail_CxPlatTlsParamSet;
    Table->CxPlatTlsProcessData = Fail_CxPlatTlsProcessData;
    Table->CxPlatTlsSecConfigCreate = Fail_CxPlatTlsSecConfigCreate;
    Table->CxPlatTlsSecConfigDelete = Fail_CxPlatTlsSecConfigDelete;
    Table->CxPlatTlsSecConfigSetTicketKeys = Fail_CxPlatTlsSecConfigSetTicketKeys;
    Table->CxPlatTlsUninitialize = Fail_CxPlatTlsUninitialize;
    Table->CxPlatTlsUpdateHkdfLabels = Fail_CxPlatTlsUpdateHkdfLabels;
    Table->CxPlatUninitialize = Fail_CxPlatUninitialize;
    Table->CxPlatUpdateRoute = Fail_CxPlatUpdateRoute;
    Table->QuicTlsPopulateOffloadKeys = Fail_QuicTlsPopulateOffloadKeys;
    Table->quic_bugcheck = Fail_quic_bugcheck;
}

static unsigned CheckMissing(MSQUIC_HOST_TABLE* Table) {
    MSQUIC_HOST_TABLE Broken;
    unsigned Count = 0;
    Broken = *Table;
    Broken.CxPlatAlloc = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatAllocUninitialized = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatConvertFromMappedV6 = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatConvertToMappedV6 = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatCurThreadID = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatDataPathGetLocalAddressForRemote = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatDataPathGetLocalAddresses = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatDataPathGetSupportedFeatures = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatDataPathInitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatDataPathIsPaddingPreferred = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatDataPathResolveAddress = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatDataPathRssConfigFree = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatDataPathRssConfigGet = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatDataPathUninitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatDataPathUpdatePollingIdleTimeout = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatDecrypt = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatEncrypt = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatEventInitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatEventQCleanup = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatEventQDequeue = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatEventQEnqueue = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatEventQInitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatEventQReturn = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatFree = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatGetAbsoluteTime = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatGetTimerResolution = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatHashCompute = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatHashCreate = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatHashFree = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatHpComputeMask = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatHpKeyCreate = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatHpKeyFree = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatInitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatInternalEventReset = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatInternalEventSet = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatInternalEventUninitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatInternalEventWaitForever = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatInternalEventWaitWithTimeout = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatKbKdfDerive = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatKeyCreate = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatKeyFree = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatLockAcquire = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatLockInitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatLockRelease = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatLockUninitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatLogAssert = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatProcCurrentNumber = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatRandom = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatRecvDataReturn = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatResolveRoute = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatResolveRouteComplete = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatRwLockAcquireExclusive = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatRwLockAcquireShared = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatRwLockInitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatRwLockReleaseExclusive = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatRwLockReleaseShared = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatRwLockUninitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSchedulerYield = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSendDataAlloc = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSendDataAllocBuffer = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSendDataFree = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSendDataFreeBuffer = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSendDataIsFull = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSleep = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSocketCreateUdp = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSocketDelete = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSocketGetLocalAddress = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSocketGetLocalMtu = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSocketGetQtipEnabled = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSocketGetRemoteAddress = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSocketSend = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSocketUpdateQeo = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSqeCleanup = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSqeInitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatStorageClose = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatStorageOpen = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatStorageReadValue = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSystemLoad = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatSystemUnload = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatThreadCreate = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatThreadDelete = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatThreadWait = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatTimeEpochMs64 = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatTimeUs64 = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatTlsExportKeyingMaterial = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatTlsGetProvider = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatTlsInitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatTlsParamGet = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatTlsParamSet = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatTlsProcessData = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatTlsSecConfigCreate = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatTlsSecConfigDelete = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatTlsSecConfigSetTicketKeys = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatTlsUninitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatTlsUpdateHkdfLabels = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatUninitialize = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.CxPlatUpdateRoute = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.QuicTlsPopulateOffloadKeys = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    Broken = *Table;
    Broken.quic_bugcheck = NULL;
    if (MsQuicHostInstall(&Broken) != QUIC_STATUS_INVALID_PARAMETER) abort();
    Count++;
    return Count;
}
