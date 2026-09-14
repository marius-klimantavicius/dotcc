#ifndef BLINK_HOST_RETAINED_THREAD_TYPES_H
#define BLINK_HOST_RETAINED_THREAD_TYPES_H
#include <stdint.h>
/* demangle.c retains a bare mutex-attribute local even under DISABLE_THREADS.
 * Its upstream disabled-thread macros remove attribute operations. This opaque
 * native-measured storage does not enable pthread headers or operations. */
typedef union { unsigned char bytes[4]; int32_t alignment; } blink_host_mutexattr_storage;
#endif
