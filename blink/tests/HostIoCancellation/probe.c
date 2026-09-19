/* Normal host callback cancellation; no guest execution or injected host fault. */
#define _POSIX_C_SOURCE 200809L
#include <stddef.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>
#include <poll.h>
#include <sys/socket.h>
#include <sys/uio.h>
#include <netinet/in.h>
#ifdef BLINK_MANAGED_CANCELLATION
#include "host-io.h"
#include "host-messages.h"
#endif
#define LENGTH 8192
#define CHECK(x) do { if (!(x)) return __LINE__; } while (0)
static long Operation(int operation, int fd, unsigned short port, unsigned char *bytes) {
  struct iovec vectors[2] = {{bytes, LENGTH / 2}, {bytes + LENGTH / 2, LENGTH / 2}};
  struct msghdr message;
  memset(&message, 0, sizeof(message));
  message.msg_iov = vectors; message.msg_iovlen = 2; message.msg_flags = 0x1357;
  struct pollfd ready = {fd, POLLIN, 0x1234};
  struct sockaddr_in address;
  memset(&address, 0, sizeof(address));
  unsigned char *raw = (unsigned char *)&address;
  raw[0] = 2; raw[2] = port >> 8; raw[3] = port; raw[4] = 127; raw[7] = 1;
  long result;
  switch (operation) {
    case 0: return read(fd, bytes, LENGTH);
    case 1: return readv(fd, vectors, 2);
    case 2: return write(fd, bytes, LENGTH);
    case 3: return writev(fd, vectors, 2);
    case 4: return recv(fd, bytes, LENGTH, 0);
    case 5: return send(fd, bytes, LENGTH, 0);
    case 6:
      result = recvmsg(fd, &message, 0);
      if (result < 0 && (message.msg_flags != 0x1357 || message.msg_iovlen != 2 || message.msg_controllen)) return -2;
      return result;
    case 7: return sendmsg(fd, &message, 0);
    case 8: return accept(fd, 0, 0);
    case 9:
      result = poll(&ready, 1, -1);
      if (result < 0 && ready.revents != 0x1234) return -2;
      return result;
    case 10: return connect(fd, (const struct sockaddr *)&address, sizeof(address));
  }
  return -2;
}
int CanceledCall(int operation, int fd, unsigned short port) {
  unsigned char bytes[LENGTH]; memset(bytes, 0xa5, sizeof(bytes));
  errno = 0;
  CHECK(Operation(operation, fd, port, bytes) == -1 && errno == ECANCELED);
  /* Failure must not commit read/recv/vector/message output bytes. */
  for (unsigned i = 0; i < sizeof(bytes); ++i) CHECK(bytes[i] == 0xa5);
  return 0;
}
long PartialWrite(int operation, int fd) {
  unsigned char bytes[LENGTH]; memset(bytes, 0x5a, sizeof(bytes));
  return Operation(operation, fd, 0, bytes);
}
int ReadFour(int fd) {
  unsigned char bytes[4] = {0};
  CHECK(read(fd, bytes, sizeof(bytes)) == 4);
  CHECK(!memcmp(bytes, "abcd", sizeof(bytes)));
  return 0;
}
int ConnectValid(int fd, unsigned short port) {
  unsigned char bytes[LENGTH]; memset(bytes, 0, sizeof(bytes));
  CHECK(Operation(10, fd, port, bytes) == 0);
  return 0;
}
long SendFour(int fd, int message_mode) {
  unsigned char bytes[] = "abcd";
  struct iovec vectors[2] = {{bytes, 2}, {bytes + 2, 2}};
  struct msghdr message; memset(&message, 0, sizeof(message));
  message.msg_iov = vectors; message.msg_iovlen = 2;
  return message_mode ? sendmsg(fd, &message, 0) : send(fd, bytes, 4, 0);
}
int LayoutProbe(void) {
  printf("C ABI iovec=%zu msghdr=%zu pollfd=%zu canceled=%d\n",
         sizeof(struct iovec), sizeof(struct msghdr), sizeof(struct pollfd), ECANCELED);
  return 0;
}
#ifndef BLINK_MANAGED_CANCELLATION
int main(void) {
  int descriptors[2]; CHECK(!pipe(descriptors));
  char source[] = "abcd", target[4] = {0};
  struct iovec send_vectors[2] = {{source, 2}, {source + 2, 2}};
  struct iovec receive_vectors[2] = {{target, 1}, {target + 1, 3}};
  CHECK(writev(descriptors[1], send_vectors, 2) == 4);
  struct pollfd ready = {descriptors[0], POLLIN, 0};
  CHECK(poll(&ready, 1, 0) == 1 && (ready.revents & POLLIN));
  CHECK(readv(descriptors[0], receive_vectors, 2) == 4 && !memcmp(source, target, 4));
  CHECK(!close(descriptors[0]) && !close(descriptors[1]));
  return LayoutProbe();
}
#endif
