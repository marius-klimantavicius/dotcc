#define _DEFAULT_SOURCE 1
#include <byteswap.h>
#include <endian.h>
#include <netdb.h>
#include <sys/uio.h>
#include <stddef.h>
#include <stdio.h>

int main(void) {
    printf("swap=%x,%x,%llx\n", (unsigned int)bswap_16(0x1234U),
           (unsigned int)bswap_32(0x12345678U),
           (unsigned long long)bswap_64(0x0123456789abcdefULL));
    unsigned int value = 0x12345678U;
    unsigned int swapped = bswap_32(value++);
    unsigned long long wide = 0x0123456789abcdefULL;
    printf("once=%x,%x roundtrip=%d,%d,%d\n", swapped, value,
           be16toh(htobe16(0x1234U)) == 0x1234U,
           be32toh(htobe32(value)) == value,
           le64toh(htole64(wide)) == wide);
    printf("narrow=%x endian=%d,%d\n", (unsigned int)htole16(0x12345678U),
           htobe32(0x12345678U) == 0x78563412U,
           be64toh(htobe64(wide)) == wide);
    printf("addrinfo=%zu,%zu,%zu,%zu,%zu\n", sizeof(struct addrinfo),
           offsetof(struct addrinfo, ai_addrlen), offsetof(struct addrinfo, ai_addr),
           offsetof(struct addrinfo, ai_canonname), offsetof(struct addrinfo, ai_next));
    printf("iovec=%zu,%zu\n", sizeof(struct iovec), offsetof(struct iovec, iov_len));
    return 0;
}
