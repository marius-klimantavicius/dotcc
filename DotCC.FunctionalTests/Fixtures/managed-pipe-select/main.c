#include <stdio.h>
#include <unistd.h>
#include <sys/select.h>
#include <poll.h>
#include <fcntl.h>
#include <errno.h>

int main(void) {
    fd_set bits;
    FD_ZERO(&bits);
    FD_SET(1023, &bits);
    printf("fdset=%lu high=%d ", (unsigned long)sizeof(bits), !!FD_ISSET(1023, &bits));
    FD_CLR(1023, &bits);
    printf("cleared=%d\n", !!FD_ISSET(1023, &bits));
    int fds[2];
    if (pipe(fds) != 0) return 1;
    fcntl(fds[0], F_SETFL, O_NONBLOCK);
    char bytes[8];
    long empty = read(fds[0], bytes, sizeof(bytes));
    printf("empty=%ld again=%d\n", empty, errno == EAGAIN);
    fd_set readers, writers;
    FD_ZERO(&readers); FD_ZERO(&writers);
    FD_SET(fds[0], &readers); FD_SET(fds[1], &writers);
    struct timeval timeout = {0, 0};
    int ready = select(fds[1] + 1, &readers, &writers, 0, &timeout);
    printf("initial=%d read=%d write=%d\n", ready, !!FD_ISSET(fds[0], &readers), !!FD_ISSET(fds[1], &writers));
    write(fds[1], "ok", 2);
    FD_SET(fds[0], &readers);
    ready = select(fds[1] + 1, &readers, &writers, 0, &timeout);
    long got = read(fds[0], bytes, sizeof(bytes));
    printf("ready=%d bytes=%ld value=%c%c\n", ready, got, bytes[0], bytes[1]);
    close(fds[1]);
    struct pollfd check = {fds[0], POLLIN, 0};
    poll(&check, 1, 0);
    ready = select(fds[0] + 1, &readers, 0, 0, &timeout);
    printf("eof=%ld ready=%d hup=%d\n", read(fds[0], bytes, sizeof(bytes)), ready, !!(check.revents & POLLHUP));
    close(fds[0]);
    return 0;
}
