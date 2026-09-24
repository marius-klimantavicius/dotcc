#define _DEFAULT_SOURCE 1
#include <sys/socket.h>
#include <netinet/in.h>
#include <unistd.h>
#include <errno.h>
#include <stdio.h>
#include <string.h>
static int option(int fd, int name) {
    int value = -1; socklen_t length = sizeof(value);
    if (getsockopt(fd, SOL_SOCKET, name, &value, &length)) return -2;
    return value;
}
static int sharing(int opt) {
    int first = socket(AF_INET, SOCK_STREAM, 0), second = socket(AF_INET, SOCK_STREAM, 0), one = 1;
    struct sockaddr_in address;
    memset(&address, 0, sizeof(address)); address.sin_family = AF_INET;
    ((unsigned char *)&address.sin_addr)[0] = 127; ((unsigned char *)&address.sin_addr)[3] = 1;
    if (first < 0 || second < 0 || setsockopt(first, SOL_SOCKET, opt, &one, 4) || setsockopt(second, SOL_SOCKET, opt, &one, 4)) return -1;
    if (bind(first, (struct sockaddr *)&address, sizeof(address)) || listen(first, 1)) return -2;
    socklen_t length = sizeof(address);
    if (getsockname(first, (struct sockaddr *)&address, &length)) return -3;
    int rc = bind(second, (struct sockaddr *)&address, length);
    int result = rc == 0 ? (listen(second, 1) == 0) : (errno == EADDRINUSE ? 0 : -4);
    close(second); close(first); return result;
}
int main(void) {
    int fd = socket(AF_INET, SOCK_STREAM, 0), one = 1, zero = 0;
    if (fd < 0 || setsockopt(fd, SOL_SOCKET, SO_REUSEADDR, &one, 4)) return 1;
    printf("address=%d,%d\n", option(fd, SO_REUSEADDR), option(fd, SO_REUSEPORT));
    if (setsockopt(fd, SOL_SOCKET, SO_REUSEPORT, &one, 4) || setsockopt(fd, SOL_SOCKET, SO_REUSEADDR, &zero, 4)) return 2;
    printf("port=%d,%d\n", option(fd, SO_REUSEADDR), option(fd, SO_REUSEPORT));
    if (setsockopt(fd, SOL_SOCKET, SO_REUSEPORT, &zero, 4)) return 3;
    printf("clear=%d,%d\n", option(fd, SO_REUSEADDR), option(fd, SO_REUSEPORT));
    int short_set = setsockopt(fd, SOL_SOCKET, SO_REUSEADDR, &one, 3);
    printf("short=%d,%d\n", short_set, errno == EINVAL);
    if (setsockopt(fd, SOL_SOCKET, SO_REUSEADDR, &one, 4)) return 4;
    unsigned char bytes[2] = {0,123}; socklen_t length = 1;
    if (getsockopt(fd, SOL_SOCKET, SO_REUSEADDR, bytes, &length)) return 5;
    printf("truncated=%d,%d,%u\n", bytes[0],bytes[1],length);
    close(fd);
    printf("sharing=%d,%d\n", sharing(SO_REUSEADDR), sharing(SO_REUSEPORT));
    return 0;
}
