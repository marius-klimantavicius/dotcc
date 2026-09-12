/* Managed CxPlat binding. No native imports and no default service implementations. */
#include "msquic_host_internal.h"

static MSQUIC_HOST_TABLE MsQuicHost;
static int MsQuicHostGate;
static uint32_t MsQuicHostState; /* 0 unbound, 1 installed, 2 library loaded */

uint32_t CxPlatProcessorCount;
uint64_t CxPlatTotalMemory;
uint16_t CxPlatTlsTPHeaderSize; /* Raw picotls parameters have no provider prefix. */
QUIC_TRACE_RUNDOWN_CALLBACK* QuicTraceRundownCallback;

static void HostLock(void)
{
    while (__sync_lock_test_and_set(&MsQuicHostGate, 1)) { }
}

static void HostUnlock(void)
{
    __sync_lock_release(&MsQuicHostGate);
}

QUIC_STATUS MsQuicHostInstall(const MSQUIC_HOST_TABLE* Table)
{
    if (!Table || Table->Size < sizeof(MSQUIC_HOST_TABLE) || Table->Version != MSQUIC_HOST_VERSION ||
        !Table->ProcessorCount || Table->ProcessorCount > UINT16_MAX || !Table->TotalMemory)
        return QUIC_STATUS_INVALID_PARAMETER;
#include "required_slots.inc"
    HostLock();
    if (MsQuicHostState == 2) {
        HostUnlock();
        return QUIC_STATUS_INVALID_STATE;
    }
    MsQuicHost = *Table;
    CxPlatProcessorCount = Table->ProcessorCount;
    CxPlatTotalMemory = Table->TotalMemory;
    __atomic_store_n(&MsQuicHostState, 1, __ATOMIC_RELEASE);
    HostUnlock();
    return QUIC_STATUS_SUCCESS;
}

QUIC_STATUS MsQuicHostUninstall(void)
{
    HostLock();
    if (MsQuicHostState == 2) {
        HostUnlock();
        return QUIC_STATUS_INVALID_STATE;
    }
    memset(&MsQuicHost, 0, sizeof(MsQuicHost));
    CxPlatProcessorCount = 0;
    CxPlatTotalMemory = 0;
    __atomic_store_n(&MsQuicHostState, 0, __ATOMIC_RELEASE);
    HostUnlock();
    return QUIC_STATUS_SUCCESS;
}

const MSQUIC_HOST_TABLE* MsQuicHostGet(void)
{
    if (!__atomic_load_n(&MsQuicHostState, __ATOMIC_ACQUIRE)) abort();
    return &MsQuicHost;
}

void CxPlatSystemLoad(void)
{
    HostLock();
    if (MsQuicHostState != 1) {
        HostUnlock();
        abort();
    }
    __atomic_store_n(&MsQuicHostState, 2, __ATOMIC_RELEASE);
    HostUnlock();
    MsQuicHost.CxPlatSystemLoad(MsQuicHost.Context);
}

void CxPlatSystemUnload(void)
{
    if (__atomic_load_n(&MsQuicHostState, __ATOMIC_ACQUIRE) != 2) abort();
    /* The caller must have drained upstream workers, callbacks and I/O first. */
    MsQuicHost.CxPlatSystemUnload(MsQuicHost.Context);
    HostLock();
    __atomic_store_n(&MsQuicHostState, 1, __ATOMIC_RELEASE);
    HostUnlock();
}
