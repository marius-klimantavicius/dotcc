/* Reduction of CPython 3.13.15 Modules/socketmodule.c:5397 selection and
   Lib/socket.py:_fallback_socketpair, using the existing shared socket service.
   This fixture does not alter a product profile or provide a Python backend. */
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <fcntl.h>
#include <unistd.h>
#include <errno.h>
#include <stdio.h>
#include <string.h>

#ifndef HAVE_SOCKET
#define socket stub_socket
static int socket(int domain, int type, int protocol)
{
    (void)domain; (void)type; (void)protocol;
    errno = ENOTSUP;
    return -1;
}
#endif

static int assertions;
#define REQUIRE(condition) do { ++assertions; if (!(condition)) { \
    fprintf(stderr, "socket selection assertion %d failed at line %d errno %d\n", \
            assertions, __LINE__, errno); return 31; } } while (0)

int main(int argc, char **argv)
{
#ifndef HAVE_SOCKET
    errno = 0;
    REQUIRE(socket(AF_INET, SOCK_STREAM, 0) == -1);
    REQUIRE(errno == ENOTSUP);
    printf("upstream absent-HAVE_SOCKET branch: %d assertions ENOTSUP\n", assertions);
    return 0;
#else
    struct sockaddr_in address = {0}, server_local = {0}, client_peer = {0};
    socklen_t length = sizeof(address);
    unsigned char request[] = {'r', '2', '2', 0, 'q'};
    unsigned char response[] = {0x00, 0x7f, 0x80, 0xff};
    unsigned char received[8];
    int listener = socket(AF_INET, SOCK_STREAM, 0);
    REQUIRE(listener >= 0);
    address.sin_family = AF_INET;
    address.sin_addr.s_addr = htonl(0x7f000001U);
    address.sin_port = 0;
    REQUIRE(sizeof(address) == 16);
    REQUIRE(bind(listener, (struct sockaddr *)&address, sizeof(address)) == 0);
    REQUIRE(listen(listener, 1) == 0);
    REQUIRE(getsockname(listener, (struct sockaddr *)&address, &length) == 0);
    REQUIRE(length == sizeof(address) && address.sin_port != 0);
    int client = socket(AF_INET, SOCK_STREAM, 0);
    REQUIRE(client >= 0);
    int flags = fcntl(client, F_GETFL);
    REQUIRE(flags >= 0);
    REQUIRE(fcntl(client, F_SETFL, flags | O_NONBLOCK) == 0);
    int connected = connect(client, (struct sockaddr *)&address, sizeof(address));
    REQUIRE(connected == 0 || (connected == -1 && (errno == EINPROGRESS || errno == EWOULDBLOCK)));
    REQUIRE(fcntl(client, F_SETFL, flags) == 0);
    int server = accept(listener, 0, 0);
    REQUIRE(server >= 0);
    length = sizeof(server_local);
    REQUIRE(getsockname(server, (struct sockaddr *)&server_local, &length) == 0);
    length = sizeof(client_peer);
    REQUIRE(getpeername(client, (struct sockaddr *)&client_peer, &length) == 0);
    REQUIRE(server_local.sin_family == client_peer.sin_family &&
            server_local.sin_addr.s_addr == client_peer.sin_addr.s_addr &&
            server_local.sin_port == client_peer.sin_port);
    REQUIRE(send(client, request, sizeof(request), 0) == sizeof(request));
    memset(received, 0x55, sizeof(received));
    REQUIRE(recv(server, received, sizeof(request), 0) == sizeof(request));
    REQUIRE(memcmp(request, received, sizeof(request)) == 0 && received[5] == 0x55);
    REQUIRE(received[0] == (unsigned char)(argc > 1 ? argv[1][0] : 'r'));
    REQUIRE(send(server, response, sizeof(response), 0) == sizeof(response));
    memset(received, 0x55, sizeof(received));
    REQUIRE(read(client, received, sizeof(response)) == sizeof(response));
    REQUIRE(memcmp(response, received, sizeof(response)) == 0 && received[4] == 0x55);
    REQUIRE(close(server) == 0);
    REQUIRE(close(client) == 0);
    REQUIRE(close(listener) == 0);
    printf("real local socket fallback: %d assertions\n", assertions);
    return 0;
#endif
}
