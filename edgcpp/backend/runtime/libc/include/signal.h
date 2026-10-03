/* Portable synchronous C signal profile v1; no OS signal registration. */
#ifndef __EDG_DOT_LIBC_SIGNAL_H
#define __EDG_DOT_LIBC_SIGNAL_H
typedef int sig_atomic_t;
#define SIGABRT 6
#define SIGFPE 8
#define SIGILL 4
#define SIGINT 2
#define SIGSEGV 11
#define SIGTERM 15
#define SIG_DFL ((void (*)(int))0)
#define SIG_IGN ((void (*)(int))1)
#define SIG_ERR ((void (*)(int))-1)
#ifdef __cplusplus
extern "C" {
#endif
void (*signal(int,void (*)(int)))(int);
int raise(int);
#ifdef __cplusplus
}
#endif
#endif
