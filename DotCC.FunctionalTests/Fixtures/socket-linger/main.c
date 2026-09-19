#include <sys/socket.h>
#include <stddef.h>
#include <stdio.h>
#include <unistd.h>
#include <errno.h>
int main(void) {
    printf("layout=%zu,%zu,%zu option=%d\n", sizeof(struct linger), _Alignof(struct linger), offsetof(struct linger, l_linger), SO_LINGER);
    int fd = socket(AF_INET, SOCK_STREAM, 0);
    if (fd < 0) return 1;
    struct linger set = { 1, 7 }, got = { 0, 0 };
    socklen_t len = sizeof(got);
    if (setsockopt(fd, SOL_SOCKET, SO_LINGER, &set, sizeof(set))) return 2;
    if (getsockopt(fd, SOL_SOCKET, SO_LINGER, &got, &len)) return 3;
    printf("enabled=%d,%d,%u\n", got.l_onoff, got.l_linger, len);
    set.l_linger = 0;
    int short_set = setsockopt(fd, SOL_SOCKET, SO_LINGER, &set, sizeof(set) - 1);
    printf("short=%d,%d\n", short_set, errno == EINVAL);
    got.l_linger = 1234; len = 4;
    if (getsockopt(fd, SOL_SOCKET, SO_LINGER, &got, &len)) return 4;
    printf("truncated=%d,%d,%u\n", got.l_onoff, got.l_linger, len);
    if (setsockopt(fd, SOL_SOCKET, SO_LINGER, &set, sizeof(set))) return 5;
    len = sizeof(got);
    if (getsockopt(fd, SOL_SOCKET, SO_LINGER, &got, &len)) return 6;
    printf("abortive=%d,%d\n", got.l_onoff, got.l_linger);
    close(fd);
    return 0;
}
