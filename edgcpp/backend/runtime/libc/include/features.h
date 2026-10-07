/* Feature declarations for the portable musl/BCL provider. This does not
   advertise a glibc version or enable host-specific structure layouts. */
#ifndef EDG_LIBC_FEATURES_H
#define EDG_LIBC_FEATURES_H
#if defined(_GNU_SOURCE)
#ifndef _BSD_SOURCE
#define _BSD_SOURCE 1
#endif
#ifndef _XOPEN_SOURCE
#define _XOPEN_SOURCE 700
#endif
#endif
#endif
