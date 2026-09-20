#include <sys/epoll.h>
#include <signal.h>
#include <stdint.h>
#include <stddef.h>
#include <errno.h>
#include <fcntl.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_EPOLL
#include "host-io.h"
#ifndef BLINK_HOST_EPOLL_H
#error reviewed private epoll header not selected
#endif
#define GetFlags(fd) blink_io_control((fd), F_GETFD, 0)
#else
#define GetFlags(fd) fcntl((fd), F_GETFD)
#endif

int EpollEventSize(void) { return sizeof(struct epoll_event); }
int EpollEventAlign(void) { return _Alignof(struct epoll_event); }
int EpollDataOffset(void) { return offsetof(struct epoll_event, data); }

/* Normal empty waits never touch either the event or surrounding bytes. */
int EpollWaitOnly(int fd, int timeout, int expected_error) {
  struct { uint64_t before; struct epoll_event event; uint64_t after; } box;
  unsigned char *bytes = (unsigned char *)&box;
  sigset_t original, baseline, temporary, observed;
  for (size_t i = 0; i < sizeof(box); ++i) bytes[i] = 0xa5;
  if (sigemptyset(&baseline) || sigaddset(&baseline, SIGUSR1) ||
      sigemptyset(&temporary) || sigaddset(&temporary, SIGUSR2) ||
      sigprocmask(SIG_SETMASK, &baseline, &original)) return 1;
  errno = EDOM;
  int (*wait_call)(int, struct epoll_event *, int, int, const sigset_t *) = epoll_pwait;
  int result = wait_call(fd, &box.event, 1, timeout, &temporary);
  int saved_error = errno;
  if (sigprocmask(SIG_SETMASK, 0, &observed)) return 2;
  int restored = sigismember(&observed, SIGUSR1) == 1 &&
                 sigismember(&observed, SIGUSR2) == 0;
  if (sigprocmask(SIG_SETMASK, &original, 0)) return 3;
  if (!restored) return 4;
  if (result != (expected_error ? -1 : 0) ||
      saved_error != (expected_error ? expected_error : EDOM)) return 5;
  for (size_t i = 0; i < sizeof(box); ++i) if (bytes[i] != 0xa5) return 6;
  return 0;
}

int EpollCommon(void) {
  int original = epoll_create1(EPOLL_CLOEXEC);
  if (original < 0 || GetFlags(original) != FD_CLOEXEC) return 10;
  int alias = dup(original);
  if (alias < 0 || GetFlags(alias) != 0 || close(original)) return 11;
  int status = EpollWaitOnly(alias, 0, 0);
  if (!status) status = EpollWaitOnly(alias, 15, 0);
  if (close(alias)) return 12;
  return status;
}

#ifdef BLINK_MANAGED_EPOLL
/* This is an explicit private limitation, never a Linux equivalence claim. */
int EpollUnsupportedRegistration(int epfd, int target) {
  struct epoll_event event;
  event.events = EPOLLIN;
  event.data.u64 = UINT64_C(0x123456789abcdef0);
  int result = epoll_ctl(epfd, EPOLL_CTL_ADD, target, &event);
  return result == -1 && errno == 95 ? 0 : 1;
}
#endif
