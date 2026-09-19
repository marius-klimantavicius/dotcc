#include <errno.h>
#include <stdio.h>
#include <string.h>
#include <sys/select.h>
#ifdef BLINK_PRIVATE_FD_SETS
#include "HostFdSets.h"
#endif

static _Thread_local fd_set saved;
static _Thread_local fd_set *saved_address;
static int Expected(int fd, int seed) { return (fd + seed) % 7 == 0 && fd % 13 != 0; }
void FdSetStore(int seed) {
  FD_ZERO(&saved);
  for (int fd = 0; fd < FD_SETSIZE; ++fd) {
    if ((fd + seed) % 7 == 0) FD_SET(fd, &saved);
    if (fd % 13 == 0) FD_CLR(fd, &saved);
  }
  saved_address = &saved;
}
int FdSetVerify(int seed) {
  if (saved_address != &saved) return 1;
  for (int fd = 0; fd < FD_SETSIZE; ++fd)
    if (!!FD_ISSET(fd, saved_address) != Expected(fd, seed)) return 2;
  return 0;
}
int FdSetProbe(void) {
  if (sizeof(fd_set) != 128 || _Alignof(fd_set) != 8 || FD_SETSIZE != 1024) return 1;
  unsigned long digest = 0;
  for (int seed = 0; seed < 19; ++seed) {
    errno = 123;
    FdSetStore(seed);
    if (FdSetVerify(seed) || errno != 123) return 2;
    fd_set copy = saved;
    FD_ZERO(&saved);
    for (int fd = 0; fd < FD_SETSIZE; ++fd) {
      int present = !!FD_ISSET(fd, &copy);
      if (present != Expected(fd, seed) || FD_ISSET(fd, &saved)) return 3;
      digest = digest * 31 + present;
    }
  }
#ifdef BLINK_PRIVATE_FD_SETS
  fd_set before = saved;
  int (*contains)(int, const fd_set *) = FD_ISSET;
  void (*set)(int, fd_set *) = FD_SET;
  void (*clear)(int, fd_set *) = FD_CLR;
  int invalid[] = {-1, 1024, 2147483647};
  for (int i = 0; i < 3; ++i) {
    errno = 0; set(invalid[i], &saved);
    if (errno != EINVAL || memcmp(&saved, &before, sizeof(saved))) return 4;
    errno = 0; clear(invalid[i], &saved);
    if (errno != EINVAL || memcmp(&saved, &before, sizeof(saved))) return 5;
    errno = 0;
    if (contains(invalid[i], &saved) || errno != EINVAL) return 6;
  }
  errno = 0; FD_ZERO(0); if (errno != EFAULT) return 7;
  errno = 0; set(0, 0); if (errno != EFAULT) return 8;
  errno = 0; clear(0, 0); if (errno != EFAULT) return 9;
  errno = 0; if (contains(0, 0) || errno != EFAULT) return 10;
#endif
  printf("fd sets bytes=%zu align=%zu descriptors=%d cases=%d digest=%lu\n",
         sizeof(fd_set), _Alignof(fd_set), FD_SETSIZE, 19 * FD_SETSIZE, digest);
  return 0;
}
#ifndef BLINK_MANAGED_FD_SETS
int main(void) { return FdSetProbe(); }
#endif
