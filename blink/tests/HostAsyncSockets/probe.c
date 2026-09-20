#include <sys/epoll.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <fcntl.h>
#include <unistd.h>
#include <stdint.h>
#include <string.h>
#include <errno.h>
#ifdef BLINK_MANAGED_EPOLL
#include "host-io.h"
#define SetFlags(fd, value) blink_io_control(fd, F_SETFL, value)
#else
#define SetFlags(fd, value) fcntl(fd, F_SETFL, value)
#endif

static const uint64_t listener_data = UINT64_C(0xfedcba9876543210);
static const uint64_t socket_data = UINT64_C(0x1234567887654321);
static const uint64_t alias_data = UINT64_C(0x8877665544332211);

static int Add(int epfd, int fd, uint32_t events, uint64_t data) {
  struct epoll_event e;
  memset(&e, 0, sizeof(e));
  e.events = events; e.data.u64 = data;
  return epoll_ctl(epfd, EPOLL_CTL_ADD, fd, &e);
}
static int Wait(int epfd, uint64_t data, uint32_t mask) {
  struct epoll_event e;
  memset(&e, 0, sizeof(e));
  int n = epoll_pwait(epfd, &e, 1, 3000, 0);
  return n == 1 && e.data.u64 == data && (e.events & mask) == mask ? 0 : 1;
}
static int Empty(int epfd) {
  struct epoll_event e;
  return epoll_pwait(epfd, &e, 1, 0, 0) == 0 ? 0 : 1;
}
static int Again(int fd) {
  char c;
  return recv(fd, &c, 1, MSG_PEEK) == -1 && errno == EAGAIN ? 0 : 1;
}
static int Connect(int listener) {
  struct sockaddr_in address;
  socklen_t size = sizeof(address);
  if (getsockname(listener, (struct sockaddr *)&address, &size)) return -1;
  int fd = socket(AF_INET, SOCK_STREAM, 0);
  if (fd < 0) return -1;
  if (connect(fd, (struct sockaddr *)&address, size)) { close(fd); return -1; }
  return fd;
}

/* Ordinary nonblocking transport: every read/accept cycle drains to EAGAIN. */
int AsyncSocketsCommon(void) {
  int listener = socket(AF_INET, SOCK_STREAM, 0);
  int epfd = epoll_create1(EPOLL_CLOEXEC);
  struct sockaddr_in address;
  memset(&address, 0, sizeof(address));
  unsigned char *p = (unsigned char *)&address;
  p[0] = AF_INET; p[4] = 127; p[7] = 1;
  if (listener < 0 || epfd < 0 || bind(listener, (struct sockaddr *)&address, sizeof(address)) ||
      listen(listener, 4) || SetFlags(listener, O_NONBLOCK)) return 1;
  if (Add(epfd, listener, EPOLLIN | EPOLLOUT | EPOLLET, listener_data)) return 2;
  if (accept(listener, 0, 0) != -1 || errno != EAGAIN || Empty(epfd)) return 3;
  int peer = Connect(listener);
  if (peer < 0 || Wait(epfd, listener_data, EPOLLIN)) return 4;
  int accepted = accept(listener, 0, 0);
  if (accepted < 0 || SetFlags(accepted, O_NONBLOCK)) return 5;
  if (accept(listener, 0, 0) != -1 || errno != EAGAIN || Empty(epfd)) return 6;
  if (Add(epfd, accepted, EPOLLIN | EPOLLOUT | EPOLLET, socket_data) ||
      Wait(epfd, socket_data, EPOLLOUT) || Empty(epfd)) return 7;
  struct linger linger = {0, 0}, observed = {-1, -1};
  socklen_t size = sizeof(observed);
  if (setsockopt(accepted, SOL_SOCKET, SO_LINGER, &linger, sizeof(linger)) ||
      getsockopt(accepted, SOL_SOCKET, SO_LINGER, &observed, &size) ||
      size != sizeof(observed) || observed.l_onoff != 0) return 8;
  char bytes[3];
  if (Again(accepted) || send(peer, "abc", 3, 0) != 3 || Wait(epfd, socket_data, EPOLLIN)) return 9;
  if (recv(accepted, bytes, 3, MSG_PEEK) != 3 || memcmp(bytes, "abc", 3) ||
      recv(accepted, bytes, 3, 0) != 3 || memcmp(bytes, "abc", 3) || Again(accepted)) return 10;
  if (Empty(epfd) || send(peer, "def", 3, 0) != 3 || Wait(epfd, socket_data, EPOLLIN) ||
      recv(accepted, bytes, 3, 0) != 3 || memcmp(bytes, "def", 3) || Again(accepted)) return 11;
  int alias = dup(accepted);
  if (alias < 0 || Add(epfd, alias, EPOLLIN | EPOLLET, alias_data) || Empty(epfd)) return 12;
  if (close(accepted) || send(peer, "ghi", 3, 0) != 3) return 13;
  struct epoll_event first, second;
  if (epoll_pwait(epfd, &first, 1, 3000, 0) != 1 ||
      epoll_pwait(epfd, &second, 1, 3000, 0) != 1 ||
      !(first.events & EPOLLIN) || !(second.events & EPOLLIN) ||
      !((first.data.u64 == socket_data && second.data.u64 == alias_data) ||
        (first.data.u64 == alias_data && second.data.u64 == socket_data))) return 14;
  if (recv(alias, bytes, 3, 0) != 3 || memcmp(bytes, "ghi", 3) || Again(alias) || close(alias)) return 15;
  int reused = socket(AF_INET, SOCK_STREAM, 0);
  if (reused != accepted || Empty(epfd) || close(reused) || close(peer)) return 16;
  peer = Connect(listener);
  if (peer < 0 || Wait(epfd, listener_data, EPOLLIN)) return 17;
  accepted = accept(listener, 0, 0);
  if (accepted < 0 || accept(listener, 0, 0) != -1 || errno != EAGAIN || Empty(epfd)) return 18;
  if (close(accepted) || close(peer)) return 19;
  /* Observed ordinary Kestrel listener shutdown: no connected/queued peers. */
  linger.l_onoff = 1; linger.l_linger = 0; size = sizeof(observed);
  if (setsockopt(listener, SOL_SOCKET, SO_LINGER, &linger, sizeof(linger)) ||
      getsockopt(listener, SOL_SOCKET, SO_LINGER, &observed, &size) ||
      size != sizeof(observed) || observed.l_onoff != 1 || observed.l_linger != 0 ||
      close(listener) || close(epfd)) return 20;
  return 0;
}
