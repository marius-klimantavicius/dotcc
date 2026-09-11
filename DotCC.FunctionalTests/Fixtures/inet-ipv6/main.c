#include <arpa/inet.h>
#include <stddef.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>

static int unchanged(const unsigned char *bytes, size_t length) {
    for (size_t i = 0; i < length; ++i) if (bytes[i] != 0xa5) return 0;
    return 1;
}

int main(void) {
    printf("layout=%zu,%zu,%zu,%zu\n", sizeof(struct in6_addr), _Alignof(struct in6_addr),
           sizeof(struct sockaddr_in6), _Alignof(struct sockaddr_in6));
    printf("offsets=%zu,%zu,%zu,%zu,%zu bytes=%zu limits=%d,%d\n",
           offsetof(struct sockaddr_in6, sin6_family), offsetof(struct sockaddr_in6, sin6_port),
           offsetof(struct sockaddr_in6, sin6_flowinfo), offsetof(struct sockaddr_in6, sin6_addr),
           offsetof(struct sockaddr_in6, sin6_scope_id), sizeof(((struct in6_addr *)0)->s6_addr),
           INET_ADDRSTRLEN, INET6_ADDRSTRLEN);
    const char *addresses[] = {"::", "::1", "2001:0DB8:0:0:0:0:0:1", "::ffff:192.0.2.128",
        "::192.0.2.128", "1:0:0:2:0:0:3:4", "1:2:3:4:5:6:7:8", "2001:db8::", "::ffff:0:192.0.2.128", "::0.0.0.2"};
    for (size_t i = 0; i < sizeof(addresses) / sizeof(addresses[0]); ++i) {
        struct sockaddr_in6 socket_address;
        memset(&socket_address, 0, sizeof(socket_address));
        socket_address.sin6_family = AF_INET6; socket_address.sin6_port = htons(443);
        errno = EBUSY;
        int parsed = inet_pton(AF_INET6, addresses[i], &socket_address.sin6_addr);
        char text[INET6_ADDRSTRLEN];
        const char *formatted = inet_ntop(AF_INET6, &socket_address.sin6_addr, text, sizeof(text));
        printf("v6=%d,%d,%s,%d\n", parsed, errno == EBUSY, formatted, ntohs(socket_address.sin6_port));
        if (i == 2) printf("network=%u,%u,%u,%u,%u\n", socket_address.sin6_addr.s6_addr[0],
            socket_address.sin6_addr.s6_addr[1], socket_address.sin6_addr.s6_addr[2],
            socket_address.sin6_addr.s6_addr[3], socket_address.sin6_addr.s6_addr[15]);
    }
    const char *invalid4[] = {"123", "127.1", "01.2.3.4", "0x7f.0.0.1", "1.2.3.256", "1.2.3.4 ", "1.2.3", "1.2.3.4.5"};
    const char *invalid6[] = {"[::1]", "::1%2", " ::1", "::1 ", ":::1", "1::2::3", "::ffff:192.000.2.1", "::ffff:127.1"};
    unsigned char storage[18]; int good = 0;
    for (int family = 0; family < 2; ++family) for (int i = 0; i < 8; ++i) {
        memset(storage, 0xa5, sizeof(storage)); errno = EBUSY;
        int result = inet_pton(family ? AF_INET6 : AF_INET, family ? invalid6[i] : invalid4[i], storage + 1);
        good += result == 0 && errno == EBUSY && unchanged(storage, sizeof(storage));
    }
    printf("strict=%d\n", good);
    memset(storage, 0xa5, sizeof(storage)); errno = EBUSY;
    int v4 = inet_pton(AF_INET, "192.0.2.128", storage + 1);
    printf("v4=%d,%d,%u,%u,%u,%u,%d\n", v4, errno == EBUSY, storage[1], storage[2], storage[3], storage[4], storage[0] == 0xa5 && storage[5] == 0xa5);
    struct in6_addr address; inet_pton(AF_INET6, "2001:db8::1", &address);
    memset(storage, 0xa5, sizeof(storage));
    const char *failed = inet_ntop(AF_INET6, &address, (char *)storage, 11);
    printf("short=%d,%d,%d\n", failed == NULL, errno == ENOSPC, unchanged(storage, sizeof(storage)));
    int unsupported = inet_pton(AF_UNSPEC, "::1", storage);
    printf("family=%d,%d,%d\n", unsupported, errno == EAFNOSUPPORT, unchanged(storage, sizeof(storage)));
    return 0;
}
