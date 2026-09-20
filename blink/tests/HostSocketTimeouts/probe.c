#include <stddef.h>
#include <stdint.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>
#include <sys/socket.h>
#include <sys/time.h>
#include <sys/uio.h>
#include <netinet/in.h>
#ifdef BLINK_MANAGED_SOCKET_TIMEOUTS
#include "host-io.h"
#include "host-messages.h"
#endif

long long TimeoutMilliseconds(void);
int TimeoutLayout(void) {
  return sizeof(struct timeval) != 16 || offsetof(struct timeval, tv_sec) != 0 ||
         offsetof(struct timeval, tv_usec) != 8 || sizeof(((struct timeval *)0)->tv_sec) != 8;
}
int TimeoutValue(int fd, int option, long seconds) {
  struct timeval actual = {-7, -9};
  socklen_t length = sizeof(actual);
  errno = EDOM;
  if (getsockopt(fd, SOL_SOCKET, option, &actual, &length) || length != 16 ||
      actual.tv_sec != seconds || actual.tv_usec != 0 || errno != EDOM) return 1;
  return 0;
}
int TimeoutSet(int fd, int option, long seconds) {
  struct timeval value = {seconds, 0};
  errno = EDOM;
  if (setsockopt(fd, SOL_SOCKET, option, &value, sizeof(value)) || errno != EDOM) return 1;
  return TimeoutValue(fd, option, seconds);
}
int TimeoutListener(void) {
  int fd = socket(AF_INET, SOCK_STREAM, 0);
  struct sockaddr_in address;
  memset(&address, 0, sizeof(address));
  unsigned char *p = (unsigned char *)&address;
  p[0] = AF_INET; p[4] = 127; p[7] = 1;
  if (fd < 0) return -1;
  if (bind(fd, (struct sockaddr *)&address, sizeof(address)) || listen(fd, 4)) {
    close(fd); return -1;
  }
  return fd;
}
int TimeoutConnect(int listener) {
  struct sockaddr_in address;
  socklen_t length = sizeof(address);
  if (getsockname(listener, (struct sockaddr *)&address, &length)) return -1;
  int fd = socket(AF_INET, SOCK_STREAM, 0);
  if (fd < 0) return -1;
  if (connect(fd, (struct sockaddr *)&address, length)) { close(fd); return -1; }
  return fd;
}

/* One normal call, retaining actual no-progress error and canary observation.
 * route0=accept,1=recv,2=read,3=readv,4=recvmsg. */
int TimeoutWait(int fd, int route, int expected_error, int timed) {
  unsigned char buffer[6]; memset(buffer, 0xa5, sizeof(buffer));
  struct iovec vectors[2] = {{buffer + 1, 2}, {buffer + 3, 2}};
  struct msghdr message; memset(&message, 0, sizeof(message));
  message.msg_iov = vectors; message.msg_iovlen = 2; message.msg_flags = 123;
  long long before = TimeoutMilliseconds();
  errno = EDOM;
  ssize_t result;
  if (route == 0) result = accept(fd, 0, 0);
  else if (route == 1) result = recv(fd, buffer + 1, 4, 0);
  else if (route == 2) result = read(fd, buffer + 1, 4);
  else if (route == 3) result = readv(fd, vectors, 2);
  else result = recvmsg(fd, &message, 0);
  int error = errno;
  long long elapsed = TimeoutMilliseconds() - before;
  if (result != -1 || error != expected_error) return 1;
  if (timed && (elapsed < 800 || elapsed > 10000)) return 2;
  for (int i = 0; i < 6; ++i) if (buffer[i] != 0xa5) return 3;
  if (route == 4 && (message.msg_flags != 123 || message.msg_iovlen != 2 ||
      message.msg_iov != vectors || message.msg_namelen || message.msg_controllen)) return 4;
  return 0;
}

/* Small ordinary stream transfers tolerate genuine short returns. No forced
 * backpressure, send expiration, disconnect, or delayed peer is manufactured. */
int TimeoutExchange(int sender, int receiver, int route) {
  const char text[] = "ordinary";
  char buffer[10]; memset(buffer, 0x5a, sizeof(buffer));
  int sent = 0;
  while (sent < 8) {
    int part = (8 - sent) / 2;
    struct iovec vectors[2] = {{(void *)(text + sent), (size_t)part},
                              {(void *)(text + sent + part), (size_t)(8 - sent - part)}};
    struct msghdr message; memset(&message, 0, sizeof(message));
    message.msg_iov = vectors; message.msg_iovlen = 2;
    ssize_t n;
    if (route == 0) n = send(sender, text + sent, 8 - sent, 0);
    else if (route == 1) n = write(sender, text + sent, 8 - sent);
    else if (route == 2) n = writev(sender, vectors, 2);
    else n = sendmsg(sender, &message, 0);
    if (n <= 0 || n > 8 - sent) return 1;
    sent += (int)n;
  }
  int received = 0;
  while (received < 8) {
    int part = (8 - received) / 2;
    struct iovec vectors[2] = {{buffer + 1 + received, (size_t)part},
                              {buffer + 1 + received + part, (size_t)(8 - received - part)}};
    struct msghdr message; memset(&message, 0, sizeof(message));
    message.msg_iov = vectors; message.msg_iovlen = 2;
    ssize_t n;
    if (route == 0) n = recv(receiver, buffer + 1 + received, 8 - received, 0);
    else if (route == 1) n = read(receiver, buffer + 1 + received, 8 - received);
    else if (route == 2) n = readv(receiver, vectors, 2);
    else n = recvmsg(receiver, &message, 0);
    if (n <= 0 || n > 8 - received) return 2;
    received += (int)n;
  }
  return memcmp(buffer + 1, text, 8) || buffer[0] != 0x5a || buffer[9] != 0x5a;
}

int TimeoutCommon(void) {
  if (TimeoutLayout()) return 10;
  int listener = TimeoutListener();
  if (listener < 0) return 11;
  if (TimeoutSet(listener, SO_SNDTIMEO, 5) || TimeoutSet(listener, SO_RCVTIMEO, 5)) return 12;
  int alias = dup(listener);
  if (alias < 0 || TimeoutSet(alias, SO_RCVTIMEO, 1) || TimeoutValue(listener, SO_RCVTIMEO, 1)) return 13;
  int status = TimeoutWait(alias, 0, EAGAIN, 1);
  if (status) return 20 + status;
  if (TimeoutSet(listener, SO_RCVTIMEO, 5) || close(alias)) return 25;
  int client = TimeoutConnect(listener);
  int server = accept(listener, 0, 0);
  if (client < 0 || server < 0) return 26;
  if (TimeoutValue(server, SO_RCVTIMEO, 5) || TimeoutValue(server, SO_SNDTIMEO, 5)) return 27;
  alias = dup(server);
  if (alias < 0 || TimeoutSet(alias, SO_RCVTIMEO, 1) || TimeoutValue(server, SO_RCVTIMEO, 1)) return 28;
  for (int route = 1; route <= 4; ++route) {
    status = TimeoutWait(alias, route, EAGAIN, 1);
    if (status) return 30 + route * 10 + status;
  }
  /* The actual service transfers with nonzero options: qualify that path
   * before separately checking reset-to-zero behavior. */
  for (int route = 0; route < 4; ++route) {
    if (TimeoutExchange(client, alias, route) || TimeoutExchange(alias, client, route)) return 75 + route;
  }
  if (TimeoutSet(alias, SO_RCVTIMEO, 0) || TimeoutValue(server, SO_RCVTIMEO, 0) ||
      TimeoutSet(server, SO_SNDTIMEO, 0) || TimeoutValue(alias, SO_SNDTIMEO, 0)) return 80;
  if (close(server)) return 81;
  for (int route = 0; route < 4; ++route) {
    if (TimeoutExchange(client, alias, route) || TimeoutExchange(alias, client, route)) return 90 + route;
  }
  if (close(alias) || close(client) || close(listener)) return 99;
  return 0;
}
