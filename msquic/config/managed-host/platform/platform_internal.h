/*++

    Copyright (c) Microsoft Corporation.
    Licensed under the MIT License.

--*/

#pragma once

#pragma warning(disable:28922) // Redundant Pointer Test
#pragma warning(disable:26451) // Arithmetic overflow: Using operator '+' on a 4 byte value and then casting the result to a 8 byte value.

#define QUIC_API_ENABLE_PREVIEW_FEATURES 1

#include "quic_platform.h"
#include "quic_datapath.h"
#include "quic_pcp.h"
#include "quic_storage.h"
#include "quic_tls.h"
#include "quic_versions.h"
#include "quic_trace.h"

#include "msquic.h"
#include "msquicp.h"

// Must be included after msquic.h for QUIC_CERTIFICATE_FLAGS
#include "quic_cert.h"

#ifdef QUIC_FUZZER
#include "msquic_fuzz.h"

#define QUIC_DISABLED_BY_FUZZER_START if (!MsQuicFuzzerContext.RedirectDataPath) {
#define QUIC_DISABLED_BY_FUZZER_END }

#else

#define QUIC_DISABLED_BY_FUZZER_START
#define QUIC_DISABLED_BY_FUZZER_END

#endif

/* No native datapath implementation types belong in the managed host ABI. */
QUIC_STATUS CxPlatCryptInitialize(void);
void CxPlatCryptUninitialize(void);

/* Upstream datapath type values retained for portable route-copy policy. */
typedef enum CXPLAT_DATAPATH_TYPE {
    CXPLAT_DATAPATH_TYPE_UNKNOWN = 0,
    CXPLAT_DATAPATH_TYPE_NORMAL,
    CXPLAT_DATAPATH_TYPE_RAW
} CXPLAT_DATAPATH_TYPE;
