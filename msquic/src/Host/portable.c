/* Copyright (c) Microsoft Corporation. Licensed under the MIT License.
   Unchanged reference/rundown implementations extracted from pinned
   src/platform/platform_posix.c. Host-owned event operations remain imports. */
#include "platform_internal.h"

void
CxPlatRefInitialize(
    _Inout_ CXPLAT_REF_COUNT* RefCount
    )
{
    *RefCount = 1;
}

void
CxPlatRefInitializeEx(
    _Inout_ CXPLAT_REF_COUNT* RefCount,
    _In_ uint32_t Initial
    )
{
    *RefCount = (int64_t)Initial;
}

void
CxPlatRefInitializeMultiple(
    _Out_writes_(Count) CXPLAT_REF_COUNT* RefCounts,
    _In_ uint32_t Count
    )
{
    for (uint32_t i = 0; i < Count; i++) {
        CxPlatRefInitialize(&RefCounts[i]);
    }
}

void
CxPlatRefIncrement(
    _Inout_ CXPLAT_REF_COUNT* RefCount
    )
{
    if (__atomic_add_fetch(RefCount, 1, __ATOMIC_SEQ_CST) > 1) {
        return;
    }

    CXPLAT_FRE_ASSERT(FALSE);
}

BOOLEAN
CxPlatRefIncrementNonZero(
    _Inout_ volatile CXPLAT_REF_COUNT* RefCount,
    _In_ uint32_t Bias
    )
{
    CXPLAT_REF_COUNT OldValue = *RefCount;

    for (;;) {
        CXPLAT_REF_COUNT NewValue = OldValue + Bias;

        if (NewValue > Bias) {
            if(__atomic_compare_exchange_n(RefCount, &OldValue, NewValue, false, __ATOMIC_SEQ_CST, __ATOMIC_SEQ_CST)) {
                return TRUE;
            }
            continue;
        }

        if (NewValue == Bias) {
            return FALSE;
        }

        CXPLAT_FRE_ASSERT(false);
        return FALSE;
    }
}

BOOLEAN
CxPlatRefDecrement(
    _In_ CXPLAT_REF_COUNT* RefCount
    )
{
    CXPLAT_REF_COUNT NewValue = __atomic_sub_fetch(RefCount, 1, __ATOMIC_SEQ_CST);

    if (NewValue > 0) {
        return FALSE;
    }

    if (NewValue == 0) {
        return TRUE;
    }

    CXPLAT_FRE_ASSERT(FALSE);

    return FALSE;
}

void
CxPlatRundownInitialize(
    _Inout_ CXPLAT_RUNDOWN_REF* Rundown
    )
{
    CxPlatRefInitialize(&((Rundown)->RefCount));
    CxPlatEventInitialize(&((Rundown)->RundownComplete), false, false);
}

void
CxPlatRundownInitializeDisabled(
    _Inout_ CXPLAT_RUNDOWN_REF* Rundown
    )
{
    (Rundown)->RefCount = 0;
    CxPlatEventInitialize(&((Rundown)->RundownComplete), false, false);
}

void
CxPlatRundownReInitialize(
    _Inout_ CXPLAT_RUNDOWN_REF* Rundown
    )
{
    (Rundown)->RefCount = 1;
}

void
CxPlatRundownUninitialize(
    _Inout_ CXPLAT_RUNDOWN_REF* Rundown
    )
{
    CxPlatEventUninitialize((Rundown)->RundownComplete);
}

BOOLEAN
CxPlatRundownAcquire(
    _Inout_ CXPLAT_RUNDOWN_REF* Rundown
    )
{
    return CxPlatRefIncrementNonZero(&(Rundown)->RefCount, 1);
}

void
CxPlatRundownRelease(
    _Inout_ CXPLAT_RUNDOWN_REF* Rundown
    )
{
    if (CxPlatRefDecrement(&(Rundown)->RefCount)) {
        CxPlatEventSet((Rundown)->RundownComplete);
    }
}

void
CxPlatRundownReleaseAndWait(
    _Inout_ CXPLAT_RUNDOWN_REF* Rundown
    )
{
    if (!CxPlatRefDecrement(&(Rundown)->RefCount)) {
        CxPlatEventWaitForever((Rundown)->RundownComplete);
    }
}

