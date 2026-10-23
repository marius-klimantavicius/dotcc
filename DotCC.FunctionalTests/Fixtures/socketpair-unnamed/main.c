#include <sys/socket.h>
#include <unistd.h>
#include <stdio.h>
#include <errno.h>
int main(void)
{
    int pair[2];
    if (socketpair(AF_UNIX, SOCK_STREAM | SOCK_NONBLOCK, 0, pair) < 0) return 1;
    unsigned char input[] = { 0, 128, 255 }, output[3] = { 0 };
    int count = (int)write(pair[0], input, 3);
    int received = (int)read(pair[1], output, 3);
    int empty = (int)read(pair[1], output, 3);
    int blocked = errno == EAGAIN;
    if (shutdown(pair[0], SHUT_WR) < 0) return 2;
    int eof = (int)read(pair[1], output, 3);
    close(pair[0]); close(pair[1]);
    printf("%d %d %d %d %d %d\n", count, received, output[2], empty, blocked, eof);
    return 0;
}
