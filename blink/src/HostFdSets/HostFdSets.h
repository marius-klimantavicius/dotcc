#ifndef BLINK_HOST_FD_SETS_H
#define BLINK_HOST_FD_SETS_H
/* Read the generic declarations once before selecting the private record.
 * The generic fd_set/FD_* placeholders are not this boundary's storage or calls. */
#include <sys/select.h>
#include <stddef.h>
typedef struct { unsigned long bits[16]; } blink_host_fd_set;
_Static_assert(sizeof(unsigned long) == 8, "private fd sets require LP64");
_Static_assert(sizeof(blink_host_fd_set) == 128, "private fd set storage");
_Static_assert(_Alignof(blink_host_fd_set) == 8, "private fd set alignment");
#undef FD_SETSIZE
#define FD_SETSIZE 1024
#undef FD_ZERO
#undef FD_SET
#undef FD_CLR
#undef FD_ISSET
#define fd_set blink_host_fd_set
#define FD_ZERO blink_host_fd_zero
#define FD_SET blink_host_fd_set_bit
#define FD_CLR blink_host_fd_clear_bit
#define FD_ISSET blink_host_fd_is_set
void blink_host_fd_zero(blink_host_fd_set *);
void blink_host_fd_set_bit(int, blink_host_fd_set *);
void blink_host_fd_clear_bit(int, blink_host_fd_set *);
int blink_host_fd_is_set(int, const blink_host_fd_set *);
#endif
