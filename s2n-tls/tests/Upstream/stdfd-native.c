#define _DEFAULT_SOURCE 1
#include <errno.h>
#include <poll.h>
#include <pthread.h>
#include <signal.h>
#include <stdint.h>
#include <stdio.h>
#include <sys/ioctl.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <unistd.h>

static pthread_t target, timer;
static void (*previous)(int);
static void no_op(int value) { (void)value; }
static void *interrupt_thread(void *value) { (void)value; usleep(100000); pthread_kill(target, SIGUSR1); return NULL; }
int start_interrupt(void) { previous = signal(SIGUSR1, no_op); target = pthread_self(); return pthread_create(&timer, NULL, interrupt_thread, NULL); }
void finish_interrupt(void) { pthread_join(timer, NULL); signal(SIGUSR1, previous); }

#ifndef STDFD_HELPER_ONLY
int main(void)
{
    int listener = socket(AF_INET, SOCK_STREAM, 0);
    struct sockaddr_in address = { .sin_family = AF_INET, .sin_addr.s_addr = htonl(INADDR_LOOPBACK) };
    if (bind(listener, (struct sockaddr *)&address, sizeof(address)) || listen(listener, 1)) return 1;
    socklen_t size = sizeof(address); if (getsockname(listener, (struct sockaddr *)&address, &size)) return 1;
    int sender = socket(AF_INET, SOCK_STREAM, 0);
    if (connect(sender, (struct sockaddr *)&address, size)) return 1;
    int receiver = accept(listener, NULL, NULL);
    unsigned char value = 69, incoming = 0, bytes[3];
    if (write(sender, &value, 1) != 1) return 2;
    struct pollfd entries[2] = { { .fd = 0, .events = POLLIN }, { .fd = receiver, .events = POLLIN } };
    int ready = poll(entries, 2, 1000);
    fprintf(stderr, "ready %d %d %d\n", ready, entries[0].revents, entries[1].revents);
    uint32_t available = 0; int queried = ioctl(0, FIONREAD, &available);
    fprintf(stderr, "available %d %u\n", queried, available);
    ssize_t got = read(receiver, &incoming, 1); fprintf(stderr, "socket %ld %u\n", got, incoming);
    for (;;) {
        ssize_t count = read(0, bytes, 3); fprintf(stderr, "read %ld\n", count);
        if (count < 0) return 3;
        if (!count) break;
        for (ssize_t sent = 0; sent < count;) { ssize_t n = write(1, bytes + sent, count - sent); if (n <= 0) return 4; sent += n; }
    }
    entries[0] = (struct pollfd) { .fd = 0, .events = POLLIN };
    fprintf(stderr, "eof %ld\n", read(0, bytes, 3)); ready = poll(entries, 1, 0);
    fprintf(stderr, "hangup %d %d\n", ready, entries[0].revents);
    entries[0] = (struct pollfd) { .fd = receiver, .events = POLLIN };
    ready = poll(entries, 1, 20); fprintf(stderr, "timeout %d %d\n", ready, entries[0].revents);
    entries[0] = (struct pollfd) { .fd = -1, .events = POLLIN }; entries[1] = (struct pollfd) { .fd = 999999, .events = POLLIN };
    errno = 77; ready = poll(entries, 2, 1000);
    fprintf(stderr, "invalid %d %d %d %d\n", ready, entries[0].revents, entries[1].revents, errno);
    ready = poll(NULL, 1, 0); fprintf(stderr, "fault %d %d\n", ready, errno);
    got = read(999999, bytes, 1); fprintf(stderr, "bad-read %ld %d\n", got, errno);
    if (start_interrupt()) return 5;
    ready = poll(NULL, 0, 1000); int error = errno; finish_interrupt();
    fprintf(stderr, "interrupt %d %d\n", ready, error);
    close(0); got = read(0, bytes, 1);
    fprintf(stderr, "closed-read %ld %d\n", got, errno);
    entries[0] = (struct pollfd) { .fd = 0, .events = POLLIN }; ready = poll(entries, 1, 0);
    fprintf(stderr, "closed-poll %d %d\n", ready, entries[0].revents);
    close(receiver); close(sender); close(listener); return 0;
}
#endif
