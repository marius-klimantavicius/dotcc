/* Portable CPU-limit request. A provider must map these values explicitly;
   declaring the API does not supply CPU-limit enforcement. */
#ifndef EDG_SELF_SYS_RESOURCE_H
#define EDG_SELF_SYS_RESOURCE_H
typedef unsigned long rlim_t;
struct rlimit { rlim_t rlim_cur, rlim_max; };
#define RLIMIT_CPU 0
#define RLIM_INFINITY ((rlim_t)-1)
#ifdef __cplusplus
extern "C" {
#endif
int getrlimit(int, struct rlimit *);
int setrlimit(int, const struct rlimit *);
#ifdef __cplusplus
}
#endif
#endif
