#pragma once
#include "msquic_host.h"

/* Table storage and context roots outlive all active core work. */
const MSQUIC_HOST_TABLE* MsQuicHostGet(void);
