/* A19: local fallback endpoint authentication, CLOEXEC and select/wakeup.
   PREPARED, UNRUN; actual Python _fallback_socketpair remains a product gate. */
#include "pyconfig.h"
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <sys/select.h>
#include <fcntl.h>
#include <unistd.h>
#include <errno.h>
#include <stdio.h>
#include <string.h>

static int assertions;
#define REQUIRE(c) do { ++assertions; if (!(c)) { \
    fprintf(stderr, "selfpipe assertion %d line %d errno %d\n", \
            assertions, __LINE__, errno); return 33; } } while (0)

static int ready(int fd, int write_ready, long seconds)
{
    fd_set descriptors;
    FD_ZERO(&descriptors);
    FD_SET(fd, &descriptors);
    struct timeval timeout = {seconds, 0};
    return select(fd + 1, write_ready ? 0 : &descriptors,
                  write_ready ? &descriptors : 0, 0, &timeout);
}

static int same_endpoint(struct sockaddr_in *a, struct sockaddr_in *b)
{
    return a->sin_family == b->sin_family && a->sin_port == b->sin_port &&
           a->sin_addr.s_addr == b->sin_addr.s_addr;
}

int main(int argc, char **argv)
{
    struct sockaddr_in address = {0}, local_client = {0}, peer_client = {0};
    struct sockaddr_in local_server = {0}, peer_server = {0};
    socklen_t length;
    address.sin_family = AF_INET;
    address.sin_addr.s_addr = htonl(0x7f000001U);
    int listener = socket(AF_INET, SOCK_STREAM | SOCK_CLOEXEC, 0);
    REQUIRE(listener >= 0 && (fcntl(listener, F_GETFD) & FD_CLOEXEC) != 0);
    REQUIRE(bind(listener, (struct sockaddr *)&address, sizeof(address)) == 0);
    REQUIRE(listen(listener, 1) == 0);
    length = sizeof(address);
    REQUIRE(getsockname(listener, (struct sockaddr *)&address, &length) == 0);
    REQUIRE(length == sizeof(address) && address.sin_port != 0);
    int client = socket(AF_INET, SOCK_STREAM | SOCK_CLOEXEC | SOCK_NONBLOCK, 0);
    REQUIRE(client >= 0 && (fcntl(client, F_GETFD) & FD_CLOEXEC) != 0);
    int status = fcntl(client, F_GETFL);
    REQUIRE(status >= 0 && (status & O_NONBLOCK) != 0);
    int connected = connect(client, (struct sockaddr *)&address, sizeof(address));
    REQUIRE(connected == 0 || (connected == -1 &&
            (errno == EINPROGRESS || errno == EWOULDBLOCK)));
    REQUIRE(ready(client, 1, 2) == 1);
    int error = -1;
    length = sizeof(error);
    REQUIRE(getsockopt(client, SOL_SOCKET, SO_ERROR, &error, &length) == 0);
    REQUIRE(error == 0 && length == sizeof(error));
    REQUIRE(ready(listener, 0, 2) == 1);
    int server = accept(listener, 0, 0);
    REQUIRE(server >= 0);
    /* CPython's _accept marks the accepted descriptor non-inheritable through
       its ordinary fcntl path when HAVE_ACCEPT4 is absent. */
    REQUIRE(fcntl(server, F_SETFD, FD_CLOEXEC) == 0);
    REQUIRE((fcntl(server, F_GETFD) & FD_CLOEXEC) != 0);
    REQUIRE(fcntl(client, F_SETFL, status & ~O_NONBLOCK) == 0);
    REQUIRE((fcntl(client, F_GETFL) & O_NONBLOCK) == 0);
    length = sizeof(local_client);
    REQUIRE(getsockname(client, (struct sockaddr *)&local_client, &length) == 0 && length == sizeof(local_client));
    length = sizeof(peer_client);
    REQUIRE(getpeername(client, (struct sockaddr *)&peer_client, &length) == 0 && length == sizeof(peer_client));
    length = sizeof(local_server);
    REQUIRE(getsockname(server, (struct sockaddr *)&local_server, &length) == 0 && length == sizeof(local_server));
    length = sizeof(peer_server);
    REQUIRE(getpeername(server, (struct sockaddr *)&peer_server, &length) == 0 && length == sizeof(peer_server));
    REQUIRE(same_endpoint(&local_client, &peer_server));
    REQUIRE(same_endpoint(&peer_client, &local_server));
    REQUIRE(ready(server, 0, 0) == 0);
    unsigned char wakeup = 0, received[2] = {0x55, 0x55};
    REQUIRE(send(client, &wakeup, 1, 0) == 1);
    REQUIRE(ready(server, 0, 2) == 1);
    REQUIRE(recv(server, received, 1, 0) == 1 &&
            received[0] == (unsigned char)(argc > 1 ? argv[1][0] : 0) && received[1] == 0x55);
    REQUIRE(ready(server, 0, 0) == 0);
    unsigned char reply = 0xff;
    REQUIRE(send(server, &reply, 1, 0) == 1);
    REQUIRE(ready(client, 0, 2) == 1);
    REQUIRE(read(client, received, 1) == 1 && received[0] == 0xff && received[1] == 0x55);
    REQUIRE(close(server) == 0 && close(client) == 0 && close(listener) == 0);
    printf("selfpipe: %d assertions\n", assertions);
    return 0;
}
