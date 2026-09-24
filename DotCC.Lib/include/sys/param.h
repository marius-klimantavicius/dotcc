#ifndef _DOTCC_SYS_PARAM_H
#define _DOTCC_SYS_PARAM_H

/* Unix compatibility parameters for dotcc's LP64, little-endian C model.
   In particular, callers of this header expect BYTE_ORDER to be available. */
#include <sys/types.h>
#include <limits.h>
#include <endian.h>

#define NBBY CHAR_BIT
#ifndef MIN
#define MIN(a, b) (((a) < (b)) ? (a) : (b))
#endif
#ifndef MAX
#define MAX(a, b) (((a) > (b)) ? (a) : (b))
#endif

#endif
