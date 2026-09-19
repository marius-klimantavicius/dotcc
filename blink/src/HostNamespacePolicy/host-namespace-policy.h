#ifndef BLINK_HOST_NAMESPACE_POLICY_H
#define BLINK_HOST_NAMESPACE_POLICY_H
#include "host-namespace.h"
#include <sys/socket.h>
#define link blink_host_link
#define linkat blink_host_linkat
#define symlink blink_host_symlink
#define symlinkat blink_host_symlinkat
#define readlink blink_host_readlink
#define readlinkat blink_host_readlinkat
#undef mkfifo
#define mkfifo blink_host_mkfifo
#undef mkfifoat
#define mkfifoat blink_host_mkfifoat
#define socketpair blink_host_socketpair
int link(const char *, const char *);
int linkat(int, const char *, int, const char *, int);
int symlink(const char *, const char *);
int symlinkat(const char *, int, const char *);
ssize_t readlink(const char *, char *, size_t);
ssize_t readlinkat(int, const char *, char *, size_t);
int mkfifo(const char *, unsigned int);
int mkfifoat(int, const char *, unsigned int);
int socketpair(int, int, int, int *);
#endif
