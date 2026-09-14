#ifndef BLINK_PRIVATE_HOST_YIELD_H
#define BLINK_PRIVATE_HOST_YIELD_H
#include <sched.h>
#define sched_yield blink_host_sched_yield
int sched_yield(void);
#endif
