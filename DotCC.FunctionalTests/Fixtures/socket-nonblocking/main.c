#define _DEFAULT_SOURCE 1
#include <sys/socket.h>
#include <netinet/in.h>
#include <poll.h>
#include <fcntl.h>
#include <unistd.h>
#include <errno.h>
#include <stdio.h>
#include <string.h>

static int run(int family) {
    int size = family == AF_INET ? sizeof(struct sockaddr_in) : sizeof(struct sockaddr_in6);
    unsigned char address[28];
    memset(address, 0, sizeof(address));
    *(unsigned short *)address = family;
    if (family == AF_INET) { address[4] = 127; address[7] = 1; }
    else address[23] = 1;
    int server = socket(family, SOCK_STREAM | SOCK_NONBLOCK, 0);
    if (server < 0 || bind(server, (struct sockaddr *)address, size) || listen(server, 4)) return 1;
    socklen_t len = size;
    if (getsockname(server, (struct sockaddr *)address, &len)) return 2;
    int client = socket(family, SOCK_STREAM | SOCK_NONBLOCK | SOCK_CLOEXEC, 0);
    int flags = (fcntl(client, F_GETFL) & O_NONBLOCK) != 0 && fcntl(client, F_GETFD) == FD_CLOEXEC;
    int connected = connect(client, (struct sockaddr *)address, len);
    if (connected && errno != EINPROGRESS) return 3;
    struct pollfd p = { client, POLLOUT, 0 };
    if (poll(&p, 1, 5000) != 1) return 4;
    int error = -1; socklen_t errorlen = sizeof(error);
    if (getsockopt(client, SOL_SOCKET, SO_ERROR, &error, &errorlen) || error) return 5;
    int peer = accept(server, NULL, NULL);
    if (peer < 0) return 6;
    char byte = 37, got = 0;
    if (send(peer, &byte, 1, 0) != 1) return 7;
    p.events = POLLIN;
    if (poll(&p, 1, 5000) != 1 || recv(client, &got, 1, 0) != 1 || got != byte) return 8;
    int again = recv(client, &got, 1, 0) == -1 && errno == EAGAIN;
    if (shutdown(peer, SHUT_WR)) return 9;
    if (poll(&p, 1, 5000) != 1 || recv(client, &got, 1, 0) != 0) return 10;
    len = 2;
    if (getpeername(client, (struct sockaddr *)address, &len)) return 11;
    printf("family=%d flags=%d again=%d accepted=%d length=%d eof=1\n", family, flags, again,
           (fcntl(peer, F_GETFL) & O_NONBLOCK) == 0, (int)len);
    close(peer); close(client); close(server);
    return 0;
}

int main(void) {
    printf("storage=%zu,%zu\n", sizeof(struct sockaddr_storage), _Alignof(struct sockaddr_storage));
    int result = run(AF_INET);
    if (result) return result;
    result = run(AF_INET6);
    if (result) return result;
    struct pollfd fds[2] = { {-1, POLLIN, 123}, {2147483647, 0, 0} };
    int ready = poll(fds, 2, 0);
    printf("invalid=%d,%d,%d\n", ready, fds[0].revents, fds[1].revents == POLLNVAL);
    return 0;
}
