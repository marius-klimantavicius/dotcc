#ifndef BLINK_HOST_FILE_MAPPING_H
#define BLINK_HOST_FILE_MAPPING_H
#include "HostMemory.h"
/* Enable only after memory-owner Begin and InstanceIo binding on this worker. */
int BlinkHostMemoryEnablePrivateFiles(void);
#endif
