/* A19: real shared-header ABI and h_errno binding control. PREPARED, UNRUN.
   Native uses the same C with host headers; emitted code uses embedded headers.
   No private declaration, native Python backend or resolver replacement. */
#include "pyconfig.h"
#include <stddef.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>

static int assertions;
#define REQUIRE(c) do { ++assertions; if (!(c)) { \
    fprintf(stderr, "resolver header assertion %d line %d errno %d\n", \
            assertions, __LINE__, errno); return 32; } } while (0)

int main(int argc, char **argv)
{
    int (*reverse)(const struct sockaddr *, socklen_t, char *, socklen_t,
                   char *, socklen_t, int) = getnameinfo;
    int (*resolve)(const char *, const char *, const struct addrinfo *,
                   struct addrinfo **) = getaddrinfo;
    void (*release)(struct addrinfo *) = freeaddrinfo;
    struct hostent *(*lookup)(const char *) = gethostbyname;
    REQUIRE(sizeof(socklen_t) == 4);
    REQUIRE(sizeof(struct addrinfo) == 48 && _Alignof(struct addrinfo) == 8);
    REQUIRE(sizeof(((struct addrinfo *)0)->ai_addrlen) == 4);
    REQUIRE(offsetof(struct addrinfo, ai_flags) == 0 && offsetof(struct addrinfo, ai_family) == 4);
    REQUIRE(offsetof(struct addrinfo, ai_socktype) == 8 && offsetof(struct addrinfo, ai_protocol) == 12);
    REQUIRE(offsetof(struct addrinfo, ai_addrlen) == 16 && offsetof(struct addrinfo, ai_addr) == 24);
    REQUIRE(offsetof(struct addrinfo, ai_canonname) == 32 && offsetof(struct addrinfo, ai_next) == 40);
    REQUIRE(sizeof(struct hostent) == 32 && _Alignof(struct hostent) == 8);
    REQUIRE(offsetof(struct hostent, h_name) == 0 && offsetof(struct hostent, h_aliases) == 8);
    REQUIRE(offsetof(struct hostent, h_addrtype) == 16 && offsetof(struct hostent, h_length) == 20);
    REQUIRE(offsetof(struct hostent, h_addr_list) == 24);
    REQUIRE(NI_NUMERICHOST == 1 && NI_NUMERICSERV == 2 && NI_NOFQDN == 4);
    REQUIRE(NI_NAMEREQD == 8 && NI_DGRAM == 16 && NI_MAXHOST == 1025 && NI_MAXSERV == 32);
    REQUIRE(EAI_BADFLAGS == -1 && EAI_NONAME == -2 && EAI_FAMILY == -6 && EAI_OVERFLOW == -12);
    REQUIRE(HOST_NOT_FOUND == 1 && TRY_AGAIN == 2 && NO_RECOVERY == 3 && NO_DATA == 4);

    int prior_h_errno = h_errno;
    h_errno = TRY_AGAIN;
    REQUIRE(h_errno == TRY_AGAIN && sizeof(h_errno) == 4);
    h_errno = prior_h_errno;
    struct sockaddr_in address = {0};
    address.sin_family = AF_INET;
    address.sin_addr.s_addr = htonl(0x7f000001U);
    address.sin_port = htons(512);
    char host[40], service[12];
    memset(host, 0x55, sizeof(host));
    memset(service, 0x55, sizeof(service));
    REQUIRE(reverse((struct sockaddr *)&address, sizeof(address), host, sizeof(host),
                    service, sizeof(service), NI_NUMERICHOST | NI_NUMERICSERV) == 0);
    REQUIRE(strcmp(host, "127.0.0.1") == 0 && host[10] == 0x55);
    REQUIRE(strcmp(service, argc > 1 ? argv[1] : "512") == 0 && service[4] == 0x55);
    struct addrinfo hints = {0}, *result = 0;
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_flags = AI_NUMERICHOST | AI_NUMERICSERV;
    REQUIRE(resolve("127.0.0.1", "512", &hints, &result) == 0 && result != 0);
    REQUIRE(result->ai_family == AF_INET && result->ai_addrlen == sizeof(address));
    struct sockaddr_in *resolved = (struct sockaddr_in *)result->ai_addr;
    REQUIRE(resolved != 0 && resolved->sin_addr.s_addr == address.sin_addr.s_addr &&
            resolved->sin_port == address.sin_port);
    release(result);
    release(0);
    struct hostent *entry = lookup("127.0.0.1");
    REQUIRE(entry != 0 && entry->h_name != 0);
    REQUIRE(entry->h_addrtype == AF_INET && entry->h_length == 4 && entry->h_addr_list != 0);
    REQUIRE(entry->h_addr_list[0] != 0 && memcmp(entry->h_addr_list[0], &address.sin_addr, 4) == 0);
    h_errno = prior_h_errno;
    printf("resolver header: %d assertions\n", assertions);
    return 0;
}
