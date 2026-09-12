/* Host binding/lifetime contract only; does not claim implemented PAL services. */
#include "callbacks.inc"
#include "host.c"
#include "forwarders.c"

static unsigned Loads;
static unsigned Unloads;

static void Load(void* Context) { Loads++; }
static void Unload(void* Context) { Unloads++; }
static unsigned int Initialize(void* Context) { return QUIC_STATUS_NOT_SUPPORTED; }
static uint64_t Clock(void* Context)
{
    uint64_t* Value = (uint64_t*)Context;
    *Value += 3;
    return 4294967296ULL + *Value;
}

#define CHECK(Condition) do { if (!(Condition)) abort(); } while (0)
#define LAYOUT(Type) printf("layout %s %lu %lu\n", #Type, sizeof(Type), _Alignof(Type))
#define OFFSET(Type, Field) printf("offset %s.%s %lu\n", #Type, #Field, offsetof(Type, Field))

int main(void)
{
    uint64_t Context = 7;
    MSQUIC_HOST_TABLE Table;
    Populate(&Table);
    Table.Context = &Context;
    Table.CxPlatSystemLoad = Load;
    Table.CxPlatSystemUnload = Unload;
    Table.CxPlatInitialize = Initialize;
    Table.CxPlatTimeUs64 = Clock;
    CHECK(MsQuicHostInstall(NULL) == QUIC_STATUS_INVALID_PARAMETER);
    Table.Size--;
    CHECK(MsQuicHostInstall(&Table) == QUIC_STATUS_INVALID_PARAMETER);
    Table.Size++;
    Table.Version++;
    CHECK(MsQuicHostInstall(&Table) == QUIC_STATUS_INVALID_PARAMETER);
    Table.Version--;
    Table.ProcessorCount = 0;
    CHECK(MsQuicHostInstall(&Table) == QUIC_STATUS_INVALID_PARAMETER);
    Table.ProcessorCount = 2;
    printf("missing slots %u\n", CheckMissing(&Table));
    CHECK(MsQuicHostInstall(&Table) == QUIC_STATUS_SUCCESS);
    CHECK(CxPlatProcessorCount == 2 && CxPlatTotalMemory == 4294967296ULL);
    CHECK(CxPlatTlsTPHeaderSize == 0);
    CxPlatSystemLoad();
    CHECK(MsQuicHostInstall(&Table) == QUIC_STATUS_INVALID_STATE);
    CHECK(MsQuicHostUninstall() == QUIC_STATUS_INVALID_STATE);
    /* Installation owns a copy; changing the source table cannot alter calls. */
    Table.CxPlatTimeUs64 = Fail_CxPlatTimeUs64;
    Table.Context = NULL;
    printf("clock %llu\n", (unsigned long long)CxPlatTimeUs64());
    CHECK(Context == 10);
    CHECK(CxPlatInitialize() == QUIC_STATUS_NOT_SUPPORTED);
    CxPlatSystemUnload();
    CHECK(Loads == 1 && Unloads == 1);
    CHECK(MsQuicHostUninstall() == QUIC_STATUS_SUCCESS);
    CHECK(!CxPlatProcessorCount && !CxPlatTotalMemory);
    CHECK(MsQuicHostUninstall() == QUIC_STATUS_SUCCESS);
    CHECK(MsQuicHostInstall(&Table) == QUIC_STATUS_SUCCESS);
    CHECK(MsQuicHostUninstall() == QUIC_STATUS_SUCCESS);
    printf("binding passed\n");
    LAYOUT(MSQUIC_HOST_TABLE);
    OFFSET(MSQUIC_HOST_TABLE, Context);
    OFFSET(MSQUIC_HOST_TABLE, TotalMemory);
    OFFSET(MSQUIC_HOST_TABLE, CxPlatAlloc);
    OFFSET(MSQUIC_HOST_TABLE, quic_bugcheck);
    LAYOUT(CXPLAT_LOCK);
    LAYOUT(CXPLAT_RW_LOCK);
    LAYOUT(CXPLAT_EVENT);
    LAYOUT(CXPLAT_THREAD);
    LAYOUT(CXPLAT_EVENTQ);
    LAYOUT(CXPLAT_CQE);
    LAYOUT(CXPLAT_SQE);
    OFFSET(CXPLAT_SQE, Completion);
    LAYOUT(CXPLAT_RUNDOWN_REF);
    LAYOUT(CXPLAT_THREAD_CONFIG);
    return 0;
}
