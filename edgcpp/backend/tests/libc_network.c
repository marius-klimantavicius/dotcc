/* Native oracle links the pinned musl network algorithms, so its text formatting
   and short-buffer behavior have the same specified provider as translated C. */
#include <arpa/inet.h>
#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>

static uint64_t digest = UINT64_C(14695981039346656037);
static void consume(const void *value, unsigned count)
{
    const unsigned char *p = (const unsigned char *)value;
    while (count--) { digest ^= *p++; digest *= UINT64_C(1099511628211); }
}
static uint32_t random_state = 314159265;
static uint32_t next(void)
{
    random_state ^= random_state << 13;
    random_state ^= random_state >> 17;
    random_state ^= random_state << 5;
    return random_state;
}
static unsigned formats;
static int format(int family, const unsigned char *address)
{
    unsigned capacity;
    for (capacity = 0; capacity <= 47; ++capacity) {
        char buffer[52];
        const char *result;
        int error;
        memset(buffer, 0x5a, sizeof buffer);
        errno = 123;
        result = inet_ntop(family, address, buffer + 2, capacity);
        error = errno;
        if (result && result != buffer + 2) return 1;
        if (buffer[0] != 0x5a || buffer[1] != 0x5a || buffer[2 + capacity] != 0x5a) return 2;
        if (result && error != 123) return 3;
        if (!result && error != (family == AF_INET || family == AF_INET6 ? ENOSPC : EAFNOSUPPORT)) return 4;
        consume(buffer, sizeof buffer);
        consume(&error, sizeof error);
        ++formats;
    }
    return 0;
}
int main(void)
{
    uint32_t i;
    unsigned char address[16];
    const char *cases[] = {"::", "::1", "::ffff:192.0.2.1", "::192.0.2.1",
        "1:0:0:2:0:0:3:4", "0:0:1:0:0:2:3:4", "1:2:3:4:5:6:0:0",
        "1:2:3:4:5:6:7:0", "2001:db8::ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"};
    for (i = 0; i < 65536; ++i) {
        uint16_t value = (uint16_t)i, wire = htons(value);
        const unsigned char *bytes = (const unsigned char *)&wire;
        if (bytes[0] != (value >> 8) || bytes[1] != (value & 255) || ntohs(wire) != value) return 10;
    }
    for (i = 0; i < 10000; ++i) {
        uint32_t value = next(), wire = htonl(value);
        const unsigned char *bytes = (const unsigned char *)&wire;
        if (bytes[0] != (value >> 24) || bytes[1] != ((value >> 16)&255) ||
            bytes[2] != ((value >> 8)&255) || bytes[3] != (value & 255) || ntohl(wire) != value) return 11;
    }
    for (i = 0; i < sizeof address; ++i) {
        if (in6addr_any.s6_addr[i] || in6addr_loopback.s6_addr[i] != (i == 15)) return 12;
    }
    if (INADDR_LOOPBACK != UINT32_C(0x7f000001)) return 13;
    for (i = 0; i < sizeof cases / sizeof cases[0]; ++i) {
        if (inet_pton(AF_INET6, cases[i], address) != 1) return 14;
        if (format(AF_INET6, address)) return 15;
    }
    for (i = 0; i < 300; ++i) {
        unsigned j;
        for (j = 0; j < sizeof address; ++j) address[j] = (unsigned char)next();
        if (format(AF_INET, address) || format(AF_INET6, address)) return 16;
    }
    if (format(-1, address)) return 17;
    printf("network: %u formatting/buffer cases, 65536 uint16 and 10000 uint32 roundtrips; %llu\n", formats, (unsigned long long)digest);
    return 0;
}
