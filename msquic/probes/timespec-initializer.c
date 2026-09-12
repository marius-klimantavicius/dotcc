/* Valid C17, reduced from CxPlatInternalEventWaitWithTimeout. */
#include <time.h>
long value(void) { struct timespec ts = {0, 0}; return ts.tv_nsec; }
