/* Same source for the native header oracle, selected-header RED and after-proof. */
#include <sys/socket.h>
#include <sys/socket.h>
#ifndef SOMAXCONN
#error "sys/socket.h must expose SOMAXCONN for CPython listen default"
#endif
_Static_assert(_Generic(SOMAXCONN, int: 1, default: 0), "SOMAXCONN is int");
_Static_assert(SOMAXCONN > 0, "positive listen backlog constant");
_Static_assert(sizeof(socklen_t) == 4, "socklen_t ABI retained");
_Static_assert(sizeof(struct sockaddr) == 16, "sockaddr ABI retained");
extern int printf(const char *, ...);
int main(void)
{
    int backlog = SOMAXCONN < 128 ? SOMAXCONN : 128;
    printf("SOMAXCONN=%d type=int default=%d socklen=%lu sockaddr=%lu\n",
           SOMAXCONN, backlog, sizeof(socklen_t), sizeof(struct sockaddr));
    return 0;
}
