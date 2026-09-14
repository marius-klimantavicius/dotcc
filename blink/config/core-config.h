#ifndef BLINK_CAMPAIGN_CORE_CONFIG_H
#define BLINK_CAMPAIGN_CORE_CONFIG_H
/* Core probe exclusions match the native oracle. Host capabilities require
 * separate contracts; do not copy native HAVE_* probes into managed code. */
#define BLINK_MANAGED_HOST_DECLARATIONS_ONLY 1
#define NOLINEAR 1
#define DISABLE_JIT 1
#define DISABLE_X87 1
#define DISABLE_THREADS 1
#define DISABLE_VFS 1
#define DISABLE_METAL 1
#define DISABLE_MMX 1
#define DISABLE_BCD 1
#define DISABLE_ROM 1
#define DISABLE_BMI2 1
/* Sockets remain selected. HAVE_FORK is intentionally absent. */
#endif
