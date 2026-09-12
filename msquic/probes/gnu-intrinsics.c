/* Primitive operations used in quic_platform_posix.h. */
long increment(volatile long *p) { return __sync_add_and_fetch(p, 1L); }
long read_value(const long *p) { return __atomic_load_n(p, __ATOMIC_RELAXED); }
unsigned int swap(unsigned int value) { return __builtin_bswap32(value); }
