#define _GNU_SOURCE
#include <sys/resource.h>
#include <sys/time.h>
#include <dlfcn.h>
#include <stddef.h>
#include <stdio.h>

int main(void) {
    struct rlimit limits = {128, RLIM_INFINITY};
    Dl_info info = {"module", 0, "symbol", 0};
    printf("%lu %lu %lu %lu %lu\n", (unsigned long)sizeof(rlim_t),
        (unsigned long)sizeof(limits), (unsigned long)_Alignof(struct rlimit),
        (unsigned long)offsetof(struct rlimit, rlim_cur), (unsigned long)offsetof(struct rlimit, rlim_max));
    printf("%lu %lu %lu %lu %lu %lu\n", (unsigned long)sizeof(info),
        (unsigned long)_Alignof(Dl_info), (unsigned long)offsetof(Dl_info, dli_fname),
        (unsigned long)offsetof(Dl_info, dli_fbase), (unsigned long)offsetof(Dl_info, dli_sname),
        (unsigned long)offsetof(Dl_info, dli_saddr));
    printf("%lu %d %d %d %s\n", limits.rlim_cur, limits.rlim_max == RLIM_INFINITY,
        RLIMIT_NOFILE, RLIMIT_NLIMITS, info.dli_sname);
    struct itimerval timer = {{1,2},{3,4}};
    printf("%lu %lu %lu %ld %d\n", (unsigned long)sizeof(timer),
        (unsigned long)_Alignof(struct itimerval), (unsigned long)offsetof(struct itimerval, it_value),
        timer.it_value.tv_usec, ITIMER_PROF);
    return 0;
}
