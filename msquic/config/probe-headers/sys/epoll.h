/* DIAGNOSTIC ONLY: Linux x64 declarations, no epoll implementation. */
#pragma once
#include <stdint.h>
typedef union epoll_data {
    void *ptr;
    int fd;
    uint32_t u32;
    uint64_t u64;
} epoll_data_t;
/* Packing deliberately omitted for parsing; NOT an ABI-validated definition. */
struct epoll_event {
    uint32_t events;
    epoll_data_t data;
};
#define EPOLL_CLOEXEC 02000000
#define EPOLLIN 1
#define EPOLLET (1U << 31)
#define EPOLL_CTL_ADD 1
#define EPOLL_CTL_DEL 2
int epoll_create1(int flags);
int epoll_wait(int fd, struct epoll_event *events, int count, int timeout);
int epoll_ctl(int fd, int op, int target, struct epoll_event *event);
