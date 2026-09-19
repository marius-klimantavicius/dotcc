#include "HostFdSets.h"
#include <errno.h>

/* Callers own readable/writable records. Out-of-range descriptors and null
 * record pointers have explicit private errors rather than indexing storage.
 * Valid operations preserve errno, like the native fd-set macros. */
static int ValidSet(int fd, const blink_host_fd_set *set) {
  if (!set) { errno = EFAULT; return 0; }
  if ((unsigned)fd >= FD_SETSIZE) { errno = EINVAL; return 0; }
  return 1;
}
void blink_host_fd_zero(blink_host_fd_set *set) {
  if (!set) { errno = EFAULT; return; }
  for (int i = 0; i < 16; ++i) set->bits[i] = 0;
}
void blink_host_fd_set_bit(int fd, blink_host_fd_set *set) {
  if (ValidSet(fd, set)) set->bits[(unsigned)fd / 64] |= 1UL << ((unsigned)fd % 64);
}
void blink_host_fd_clear_bit(int fd, blink_host_fd_set *set) {
  if (ValidSet(fd, set)) set->bits[(unsigned)fd / 64] &= ~(1UL << ((unsigned)fd % 64));
}
int blink_host_fd_is_set(int fd, const blink_host_fd_set *set) {
  if (!ValidSet(fd, set)) return 0;
  return !!(set->bits[(unsigned)fd / 64] & (1UL << ((unsigned)fd % 64)));
}
