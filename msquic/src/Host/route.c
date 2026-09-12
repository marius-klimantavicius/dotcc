/* Copyright (c) Microsoft Corporation. Licensed under the MIT License.
   Unchanged portable route-copy policy extracted from pinned datapath_xplat.c. */
#include "platform_internal.h"

_IRQL_requires_max_(PASSIVE_LEVEL)
void
QuicCopyRouteInfo(
    _Inout_ CXPLAT_ROUTE* DstRoute,
    _In_ CXPLAT_ROUTE* SrcRoute
    )
{
    if (SrcRoute->DatapathType == CXPLAT_DATAPATH_TYPE_RAW) {
        CxPlatCopyMemory(DstRoute, SrcRoute, (uint8_t*)&SrcRoute->State - (uint8_t*)SrcRoute);
        CxPlatUpdateRoute(DstRoute, SrcRoute);
    } else if (SrcRoute->DatapathType == CXPLAT_DATAPATH_TYPE_NORMAL) {
        *DstRoute = *SrcRoute;
    } else {
        CXPLAT_DBG_ASSERT(FALSE);
    }
}

