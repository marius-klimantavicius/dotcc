#ifndef BLINK_MANAGED_HOST_CONFIG_H
#define BLINK_MANAGED_HOST_CONFIG_H
/* Explicit campaign profile. No host OS/compiler identity macros and no
 * unproven HAVE_* platform capabilities. Declarations are not a host runtime. */
#define BLINK_MANAGED_HOST_DECLARATIONS_ONLY 1
#define NOLINEAR 1
#define DISABLE_JIT 1
#define DISABLE_X87 1
#define DISABLE_THREADS 1
#define DISABLE_MMX 1
#define DISABLE_BMI2 1
#define DISABLE_METAL 1
#define DISABLE_BCD 1
#define DISABLE_ROM 1
#define DISABLE_VFS 1
/* HAVE_FORK intentionally absent; socket and trace code remain selected. */
#endif
