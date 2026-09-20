#ifndef BLINK_HOST_EPOLL_H
#define BLINK_HOST_EPOLL_H
#include "abi.h"
#include <stddef.h>
/* Private callback ABI, not the packed Linux guest epoll_event record. */
typedef union blink_host_epoll_data {
  void *ptr;
  int fd;
  uint32_t u32;
  uint64_t u64;
} blink_host_epoll_data;
struct blink_host_epoll_event {
  uint32_t events;
  blink_host_epoll_data data;
};
_Static_assert(sizeof(struct blink_host_epoll_event) == 16, "private epoll size");
_Static_assert(_Alignof(struct blink_host_epoll_event) == 8, "private epoll alignment");
_Static_assert(offsetof(struct blink_host_epoll_event, data) == 8, "private epoll data");
#define epoll_event blink_host_epoll_event
typedef blink_host_epoll_data epoll_data_t;
#define EPOLL_CLOEXEC 0x80000
#define EPOLL_CTL_ADD 1
#define EPOLL_CTL_DEL 2
#define EPOLL_CTL_MOD 3
/* Event requests are not implemented by this empty-set profile. */
#define EPOLLIN 0x001
#define EPOLLOUT 0x004
#define EPOLLERR 0x008
#define EPOLLHUP 0x010
#define EPOLLRDHUP 0x2000
#define EPOLLONESHOT (1u << 30)
#define EPOLLET (1u << 31)
int blink_host_epoll_create1(int);
int blink_host_epoll_ctl(int, int, int, struct blink_host_epoll_event *);
int blink_host_epoll_pwait(int, struct blink_host_epoll_event *, int, int,
                          const blink_host_sigset *);
#define epoll_create1 blink_host_epoll_create1
#define epoll_ctl blink_host_epoll_ctl
#define epoll_pwait blink_host_epoll_pwait
#endif
