/* Real TCP listener; the external driver inspects its kernel queue, then acts
 * as an independent peer. This same C is reserved for fresh-tool after-controls. */
#include <sys/socket.h>
#include <poll.h>
#include <errno.h>
extern int printf(const char *, ...);
extern int getchar(void);
extern int atoi(const char *);
extern int close(int);
#if defined(SOCKET_LISTEN_NATIVE)
/* Native TLS errno accessor, not a replacement errno object. */
extern int *__errno_location(void);
#ifndef errno
#define errno (*__errno_location())
#endif
#endif

#define REQUIRE(c) do { if (!(c)) { printf("FAIL line=%d errno=%d\n", __LINE__, errno); return 31; } } while (0)

int main(int argc, char **argv)
{
    int backlog = SOMAXCONN < 128 ? SOMAXCONN : 128;
    if (argc > 1) backlog = atoi(argv[1]);
    /* This is the unchanged upstream sock_listen normalization. */
    if (backlog < 0) backlog = 0;
    struct sockaddr address = {0}, peer = {0};
    unsigned char *bytes = (unsigned char *)&address;
    address.sa_family = AF_INET;
    bytes[4] = 127; bytes[7] = 1;
    int listener = socket(AF_INET, SOCK_STREAM, 0);
    REQUIRE(listener >= 0);
    REQUIRE(bind(listener, &address, sizeof(address)) == 0);
    REQUIRE(listen(listener, backlog) == 0);
    socklen_t length = sizeof(address);
    REQUIRE(getsockname(listener, &address, &length) == 0 && length == 16);
    int port = bytes[2] * 256 + bytes[3];
    REQUIRE(port > 0);
    printf("READY port=%d backlog=%d\n", port, backlog);
    REQUIRE(getchar() == 'g');
    struct pollfd ready = {listener, POLLIN, 0};
    REQUIRE(poll(&ready, 1, 2000) == 1 && (ready.revents & POLLIN));
    length = sizeof(peer);
    int accepted = accept(listener, &peer, &length);
    REQUIRE(accepted >= 0 && length == 16 && peer.sa_family == AF_INET);
    unsigned char *remote = (unsigned char *)&peer;
    REQUIRE(remote[4] == 127 && remote[5] == 0 && remote[6] == 0 && remote[7] == 1);
    unsigned char message[2] = {0, 0};
    REQUIRE(recv(accepted, message, 2, MSG_WAITALL) == 2);
    REQUIRE(message[0] == 0x5a && message[1] == 0xa5);
    REQUIRE(send(accepted, message, 2, 0) == 2);
    REQUIRE(close(accepted) == 0 && close(listener) == 0);
    errno = 0;
    REQUIRE(listen(-1, backlog) == -1 && errno == EBADF);
    int datagram = socket(AF_INET, SOCK_DGRAM, 0);
    REQUIRE(datagram >= 0);
    errno = 0;
    REQUIRE(listen(datagram, backlog) == -1 && errno == EOPNOTSUPP);
    REQUIRE(close(datagram) == 0);
    printf("PASS backlog=%d echo=5aa5 badfd=9 datagram=95\n", backlog);
    return 0;
}
