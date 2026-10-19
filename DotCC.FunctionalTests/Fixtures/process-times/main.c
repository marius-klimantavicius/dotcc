/* Real POSIX process/children CPU accounting. Linux LP64 control; deliberately
   distinct from clock() and CLOCKS_PER_SEC's existing millisecond contract. */
#include <sys/times.h>
#include <unistd.h>
#include <time.h>
#include <errno.h>
#include <stddef.h>
#include <stdio.h>
#ifdef DOTCC_NATIVE_CHILD_CONTROL
#include <sys/wait.h>
#include <stdlib.h>
#endif

static volatile unsigned long work_value = 1;
static int require(int condition, int line) {
    if (!condition) { fprintf(stderr, "times assertion line %d (errno=%d)\n", line, errno); return 0; }
    return 1;
}
#define REQUIRE(x) do { if (!require((x), __LINE__)) return 1; } while (0)

int main(void) {
    REQUIRE(sizeof(clock_t) == 8);
    REQUIRE(sizeof(struct tms) == 32);
    REQUIRE(_Alignof(struct tms) == 8);
    REQUIRE(offsetof(struct tms, tms_utime) == 0);
    REQUIRE(offsetof(struct tms, tms_stime) == 8);
    REQUIRE(offsetof(struct tms, tms_cutime) == 16);
    REQUIRE(offsetof(struct tms, tms_cstime) == 24);
    long ticks = sysconf(_SC_CLK_TCK);
    REQUIRE(ticks > 0);
    struct { struct tms value; unsigned long sentinel; } before, after;
    before.sentinel = 0x12345678;
    after.sentinel = 0x76543210;
    errno = EIO;
    clock_t elapsed = times(&before.value);
    REQUIRE(elapsed != (clock_t)-1);
    REQUIRE(errno == EIO);
    REQUIRE(before.value.tms_utime >= 0 && before.value.tms_stime >= 0);
    REQUIRE(before.value.tms_cutime >= 0 && before.value.tms_cstime >= 0);
    struct timespec start, now;
    REQUIRE(clock_gettime(CLOCK_MONOTONIC, &start) == 0);
    for (;;) {
        for (int i = 0; i < 100000; ++i) work_value = work_value * 1664525 + 1013904223;
        REQUIRE(times(&after.value) >= elapsed);
        if (after.value.tms_utime + after.value.tms_stime >= before.value.tms_utime + before.value.tms_stime + 2) break;
        REQUIRE(clock_gettime(CLOCK_MONOTONIC, &now) == 0);
        REQUIRE(now.tv_sec - start.tv_sec < 8);
    }
    REQUIRE(before.sentinel == 0x12345678 && after.sentinel == 0x76543210);
    REQUIRE(after.value.tms_utime >= before.value.tms_utime);
    REQUIRE(after.value.tms_stime >= before.value.tms_stime);
    clock_t waited = times(NULL);
    REQUIRE(waited >= elapsed);
    REQUIRE(usleep(30000) == 0);
    REQUIRE(times(NULL) >= waited + ticks / 100);
    errno = 0;
    REQUIRE(sysconf(-123) == -1 && errno == EINVAL);
#ifdef DOTCC_NATIVE_CHILD_CONTROL
    struct tms child_before, child_after;
    REQUIRE(times(&child_before) != (clock_t)-1);
    int child = fork();
    REQUIRE(child >= 0);
    if (child == 0) {
        struct tms c;
        if (times(&c) == (clock_t)-1) _Exit(2);
        clock_t target = c.tms_utime + c.tms_stime + 3;
        do {
            for (int i = 0; i < 100000; ++i) work_value = work_value * 1664525 + 1013904223;
            if (times(&c) == (clock_t)-1) _Exit(3);
        } while (c.tms_utime + c.tms_stime < target);
        _Exit(0);
    }
    int status;
    REQUIRE(waitpid(child, &status, 0) == child && WIFEXITED(status) && WEXITSTATUS(status) == 0);
    REQUIRE(times(&child_after) != (clock_t)-1);
    REQUIRE(child_after.tms_cutime + child_after.tms_cstime >= child_before.tms_cutime + child_before.tms_cstime + 2);
    puts("real waited-child CPU ticks observed (native control only)");
#endif
    puts("real times: LP64 layout, CPU work, elapsed units, sentinel, null, sysconf errors");
    return 0;
}
