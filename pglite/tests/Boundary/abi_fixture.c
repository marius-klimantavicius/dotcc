/* Boundary experiment only. This is not a PostgreSQL implementation. */
#define _POSIX_C_SOURCE 200809L
#include <stddef.h>
#include <stdint.h>
#include <sys/types.h>
#include <stdlib.h>
#include <string.h>
#include <setjmp.h>
#include <time.h>
#include <sys/time.h>
#include <signal.h>

/* Same C signature as the selected fork's pgl_read_t / pgl_write_t. */
typedef ssize_t (*dpg_io_callback)(void *buffer, size_t length);
typedef struct dpg_fixture_layout {
    unsigned int pointer_bytes;
    unsigned int long_bytes;
    unsigned int size_bytes;
    unsigned int ssize_bytes;
    unsigned int time_bytes;
    unsigned int jump_bytes;
    unsigned int callback_bytes;
    unsigned int sample_bytes;
    unsigned int sample_payload_offset;
} dpg_fixture_layout;

typedef struct dpg_fixture_sample {
    unsigned char kind;
    size_t length;
    void *payload;
} dpg_fixture_sample;

dpg_fixture_layout dpg_fixture_get_layout(void) {
    dpg_fixture_layout result;
    result.pointer_bytes = sizeof(void *);
    result.long_bytes = sizeof(long);
    result.size_bytes = sizeof(size_t);
    result.ssize_bytes = sizeof(ssize_t);
    result.time_bytes = sizeof(time_t);
    result.jump_bytes = sizeof(jmp_buf);
    result.callback_bytes = sizeof(dpg_io_callback);
    result.sample_bytes = sizeof(dpg_fixture_sample);
    result.sample_payload_offset = offsetof(dpg_fixture_sample, payload);
    return result;
}

ssize_t dpg_fixture_invoke(dpg_io_callback callback, void *buffer, size_t length) {
    return callback(buffer, length);
}

/* Exercise the supported lexical nested unwind, rather than a stale frame. */
int dpg_fixture_nested_jump(void) {
    jmp_buf outer;
    jmp_buf inner;
    volatile int reached = 0;
    int status = setjmp(outer);
    if (status == 0) {
        int nested = setjmp(inner);
        if (nested == 0) {
            reached = 1;
            longjmp(inner, 0);
        }
        if (nested != 1) return -1;
        reached = 2;
        longjmp(outer, 7);
    }
    return status == 7 && reached == 2 ? 1 : -2;
}

/* Stable malloc storage and explicit-length UTF-8 bytes (including NUL). */
int dpg_fixture_memory(void) {
    unsigned char source[6] = {0xce, 0xbb, 0, 0xe9, 0x9b, 0xaa};
    void *owned = malloc(sizeof(source));
    int equal;
    if (!owned) return -1;
    memcpy(owned, source, sizeof(source));
    equal = memcmp(owned, source, sizeof(source)) == 0;
    free(owned);
    return equal;
}

void dpg_fixture_exit(int status) { exit(status); }

int dpg_fixture_clear_timer(void) {
    struct itimerval zero = {{0, 0}, {0, 0}};
    return setitimer(ITIMER_REAL, &zero, NULL);
}

/* Query only: no host signal handler or signal mask is changed. */
int dpg_fixture_signal_query(void) {
    struct sigaction old_action;
    unsigned char before[sizeof(old_action)];
    memset(&old_action, 0xa5, sizeof(old_action));
    memcpy(before, &old_action, sizeof(old_action));
    if (sigaction(SIGALRM, NULL, &old_action) != 0) return -1;
    return memcmp(before, &old_action, sizeof(old_action)) == 0 ? 1 : 0;
}

static int initialized_global = 37;
int dpg_fixture_next_global(void) {
    static int initialized_local = 71;
    return initialized_global++ * 100 + initialized_local++;
}

static volatile sig_atomic_t timer_calls;
static void timer_handler(int signal_number) {
    if (signal_number == SIGALRM) timer_calls++;
}
int dpg_fixture_install_alarm(void) {
    struct sigaction action;
    memset(&action, 0, sizeof(action));
    action.sa_handler = timer_handler;
    sigemptyset(&action.sa_mask);
    timer_calls = 0;
    return sigaction(SIGALRM, &action, NULL);
}
int dpg_fixture_arm_timer(long microseconds, long interval) {
    struct itimerval timer = {{0, interval}, {0, microseconds}};
    return setitimer(ITIMER_REAL, &timer, NULL);
}
int dpg_fixture_timer_calls(void) { return timer_calls; }


#ifdef DPG_NATIVE_MAIN
#include <stdio.h>
static ssize_t native_callback(void *buffer, size_t length) {
    unsigned char *bytes = buffer;
    if (bytes[0] != 0xce || bytes[1] != 0xbb || bytes[2] != 0) return -2;
    bytes[3] = 42;
    return length == 0 ? -1 : (ssize_t)length;
}
int main(void) {
    dpg_fixture_layout value = dpg_fixture_get_layout();
    unsigned char buffer[4] = {0xce, 0xbb, 0, 0};
    size_t wide = ((size_t)1 << 33) + 9;
    if (dpg_fixture_nested_jump() != 1 || dpg_fixture_memory() != 1) return 1;
    if (dpg_fixture_invoke(native_callback, buffer, wide) != (ssize_t)wide || buffer[3] != 42) return 2;
    if (dpg_fixture_invoke(native_callback, buffer, 0) != -1) return 3;
    if (dpg_fixture_signal_query() != 0) return 4;
    printf("{\"pointer\":%u,\"long\":%u,\"size_t\":%u,\"ssize_t\":%u,\"time_t\":%u,\"jmp_buf\":%u,\"callback\":%u,\"sample\":%u,\"payload_offset\":%u,\"nested_jump\":1,\"memory\":1,\"callback_roundtrip\":1}\n",
           value.pointer_bytes, value.long_bytes, value.size_bytes, value.ssize_bytes,
           value.time_bytes, value.jump_bytes, value.callback_bytes, value.sample_bytes,
           value.sample_payload_offset);
    return 0;
}
#endif
