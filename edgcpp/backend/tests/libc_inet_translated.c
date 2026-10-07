#include <arpa/inet.h>
#include <errno.h>
#include <pthread.h>
#include <stddef.h>
#include <string.h>
_Static_assert(sizeof(struct in_addr) == 4 && _Alignof(struct in_addr) == 4, "IPv4 layout");
_Static_assert(sizeof(struct in6_addr) == 16 && _Alignof(struct in6_addr) == 4, "IPv6 layout");
_Static_assert(sizeof(struct sockaddr_in) == 16 && offsetof(struct sockaddr_in, sin_addr) == 4, "IPv4 socket layout");
_Static_assert(sizeof(struct sockaddr_in6) == 28 && offsetof(struct sockaddr_in6, sin6_addr) == 8 && offsetof(struct sockaddr_in6, sin6_scope_id) == 24, "IPv6 socket layout");
_Static_assert(sizeof(struct sockaddr_storage) == 128 && _Alignof(struct sockaddr_storage) == 8, "socket storage layout");
static int check(int family, const char *source, const unsigned char *expected, size_t size) {
    unsigned char bytes[20];
    memset(bytes, 0xa5, sizeof(bytes));
    errno = 123;
    int status = inet_pton(family, source, bytes + 2);
    if (status != (expected != NULL) || errno != 123) return 1;
    for (size_t i = 0; i < sizeof(bytes); ++i)
        if (bytes[i] != (expected && i >= 2 && i < size + 2 ? expected[i - 2] : 0xa5)) return 2;
    return 0;
}
int main(void) {
    static const unsigned char v4[4] = {127, 0, 0, 1};
    static const unsigned char v6[16] = {0x20, 1, 0x0d, 0xb8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1};
    if (check(AF_INET, "127.0.0.1", v4, 4)) return 1;
    if (check(AF_INET6, "2001:db8::1", v6, 16)) return 2;
    if (check(AF_INET, "0177.0.0.1", NULL, 0)) return 3;
    if (check(AF_INET6, "fe80::1%2", NULL, 0)) return 4;
    errno = 123;
    unsigned char unsupported[16];
    if (inet_pton(-1, "::1", unsupported) != -1 || errno != EAFNOSUPPORT) return 5;
    return 0;
}
