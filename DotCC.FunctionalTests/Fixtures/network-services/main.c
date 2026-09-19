#define _DEFAULT_SOURCE 1
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>
#include <errno.h>
#include <netdb.h>
#include <netinet/in.h>
#include <sys/uio.h>
#include <sys/random.h>
#include <unistd.h>
int main(void) {
    struct addrinfo hints = {0}, *result = NULL;
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_flags = AI_NUMERICHOST | AI_NUMERICSERV;
    int code = getaddrinfo("127.0.0.1", "445", &hints, &result);
    if (code || !result || result->ai_addrlen != 16 ||
        result->ai_family != AF_INET || result->ai_protocol != 6) return 1;
    struct sockaddr_in *address = (struct sockaddr_in *)result->ai_addr;
    if (ntohs(address->sin_port) != 445 || ntohl(address->sin_addr.s_addr) != 0x7f000001) return 2;
    freeaddrinfo(result);
    result = NULL;
    if (getaddrinfo("bad!host", "445", &hints, &result) != EAI_NONAME || result) return 3;
    unsigned char bytes[256], other[257];
    memset(bytes, 0, sizeof(bytes));
    if (getrandom(bytes, sizeof(bytes), 0) != 256) return 4;
    if (getentropy(other, 256) || !memcmp(bytes, other, 256)) return 5;
    if (getentropy(other, 257) != -1 || errno != EIO) return 6;
    if (getrandom(other, 16, 0x80000000U) != -1 || errno != EINVAL) return 7;
    FILE *file = tmpfile();
    if (!file) return 8;
    int fd = fileno(file);
    char first[] = "abc";
    char second[] = "defg";
    char output[7];
    struct iovec write_vec[3] = {{first, 3}, {NULL, 0}, {second, 4}};
    struct iovec read_vec[2] = {{output, 2}, {output + 2, 5}};
    if (writev(fd, write_vec, 3) != 7 || lseek(fd, 0, SEEK_SET) != 0 ||
        readv(fd, read_vec, 2) != 7 || memcmp(output, "abcdefg", 7)) return 9;
    if (readv(fd, read_vec, 2) != 0) return 10;
    volatile int invalid_count = -1;
    if (writev(fd, write_vec, invalid_count) != -1 || errno != EINVAL) return 11;
    fclose(file);
    puts("network services passed");
    return 0;
}
