#ifndef EDG_POSIX_SYS_TIME_H
#define EDG_POSIX_SYS_TIME_H
struct timeval { long tv_sec, tv_usec; };
#ifdef __cplusplus
extern "C" {
#endif
int gettimeofday(struct timeval *, void *);
int utimes(const char *, const struct timeval [2]);
#ifdef __cplusplus
}
#endif
#endif
