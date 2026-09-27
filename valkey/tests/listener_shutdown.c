/* Test-only reduction: actual upstream select backend and listener-close body. */
#include <stdarg.h>
#include <stdio.h>
#include <stdlib.h>
#include <sys/socket.h>
#include <unistd.h>
#include <errno.h>
#include <string.h>
#include "ae.h"

#define zmalloc malloc
#define zfree free
static void panic(const char *format, ...) {
    va_list args; va_start(args, format); vfprintf(stderr, format, args); va_end(args);
    fputc('\n', stderr); exit(42);
}
#include "ae_select.c"

/* This reduction schedules the two actions serially; the complete product uses
   upstream AE_LOCK around the same real deletion and worker poll operations. */
#define AE_LOCK(eventLoop) ((void)(eventLoop))
#define AE_UNLOCK(eventLoop) ((void)(eventLoop))
#include "listener-events.inc"
#define CONN_TYPE_MAX 1
#define LL_NOTICE 0
#define LL_WARNING 1
typedef struct connListener { void *ct; int count; int fd[1]; } connListener;
static struct {
    connListener listeners[CONN_TYPE_MAX], clistener;
    int cluster_enabled;
    char *unixsocket;
    aeEventLoop *el;
} server;
static void serverLog(int level, const char *format, ...) { (void)level; (void)format; }
#include "listener-close.inc"

int main(void) {
    aeEventLoop eventLoop = {0};
    aeFileEvent events[FD_SETSIZE] = {0};
    aeFiredEvent fired[FD_SETSIZE] = {0};
    eventLoop.events = events; eventLoop.fired = fired;
    eventLoop.setsize = FD_SETSIZE;
    eventLoop.maxfd = -1;
    if (aeApiCreate(&eventLoop)) return 1;
    server.el = &eventLoop;
    for (int cluster = 0; cluster < 2; cluster++) {
        int fd = socket(AF_INET, SOCK_STREAM, 0);
        if (fd < 0 || fd >= FD_SETSIZE) return 1;
        connListener *listener = cluster ? &server.clistener : &server.listeners[0];
        listener->ct = listener; listener->count = 1; listener->fd[0] = fd;
        server.cluster_enabled = cluster;
        if (fd > eventLoop.maxfd) eventLoop.maxfd = fd;
        if (aeApiAddEvent(&eventLoop, fd, AE_READABLE)) return 1;
        events[fd].mask = AE_READABLE;
    }
    /* Deterministically schedule a queued I/O poll after shutdown closes both
       ordinary and cluster listeners, as the independent worker can do. */
    closeListeningSockets(0);
    struct timeval timeout = {0, 0};
    if (aeApiPoll(&eventLoop, &timeout) != 0) return 1;
    aeApiFree(&eventLoop);
    puts("listener shutdown: queued select poll has no stale descriptors");
    return 0;
}
