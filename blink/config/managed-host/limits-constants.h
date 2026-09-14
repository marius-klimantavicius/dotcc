#ifndef BLINK_MANAGED_HOST_LIMITS_CONSTANTS_H
#define BLINK_MANAGED_HOST_LIMITS_CONSTANTS_H
/* Native-measured Linux host storage limits used by overlays.c and log.c.
 * These constants do not implement path access or atomic pipe writes. */
#define PATH_MAX (4096)
#define PIPE_BUF (4096)
#endif
