/* Portable scheduling subset. CPU affinity layouts and operations are not
   declared or simulated by this boundary. */
#ifndef EDG_POSIX_SCHED_H
#define EDG_POSIX_SCHED_H
#ifdef __cplusplus
extern "C" {
#endif
int sched_getcpu(void);
int sched_yield(void);
#ifdef __cplusplus
}
#endif
#endif
