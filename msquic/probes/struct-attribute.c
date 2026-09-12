/* GNU C, reduced from CXPLAT_POOL_HEADER in quic_platform_posix.h. */
typedef struct __attribute__((aligned(16))) PoolHeader {
    void *next;
} PoolHeader;
unsigned long header_size(void) { return sizeof(PoolHeader); }
