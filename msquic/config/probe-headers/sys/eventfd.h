/* DIAGNOSTIC ONLY: declarations, no eventfd implementation. */
#pragma once
#include <stdint.h>
typedef uint64_t eventfd_t;
#define EFD_CLOEXEC 02000000
int eventfd(unsigned int initval, int flags);
int eventfd_write(int fd, eventfd_t value);
int eventfd_read(int fd, eventfd_t *value);
