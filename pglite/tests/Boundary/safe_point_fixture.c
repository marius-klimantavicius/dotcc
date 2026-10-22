/* Diagnostic observers around the actual adapted CHECK_FOR_INTERRUPTS macro.
 * This does not implement PostgreSQL ProcessInterrupts or any database service.
 * Managed SIGALRM can fire only through the typed poll hook inside this C loop.
 */
#define _POSIX_C_SOURCE 200809L
#include <signal.h>
#include <sys/time.h>
#include <string.h>

static volatile sig_atomic_t safe_fired, safe_pending;
static int safe_processed;
static void safe_alarm(int sig) {
    if (sig == SIGALRM) { safe_fired++; safe_pending = 1; }
}
static void observe_process_interrupts(void) {
    safe_processed++;
    safe_pending = 0;
}
#define INTERRUPTS_PENDING_CONDITION() safe_pending
#define ProcessInterrupts observe_process_interrupts
#include "dpg_safe_point.h"
#undef ProcessInterrupts

int dpg_fixture_safe_point_timer(void) {
    struct sigaction action, previous;
    struct itimerval timer = {{0, 0}, {0, 1000}}, zero = {{0, 0}, {0, 0}};
    int checks = 0;
    safe_fired = safe_pending = 0;
    safe_processed = 0;
    memset(&action, 0, sizeof(action));
    action.sa_handler = safe_alarm;
    sigemptyset(&action.sa_mask);
    if (sigaction(SIGALRM, &action, &previous) != 0) return -1;
    if (setitimer(ITIMER_REAL, &timer, NULL) != 0) {
        sigaction(SIGALRM, &previous, NULL);
        return -2;
    }
    while (!safe_fired && checks < 10000000) {
        checks++;
        /* Expand the unchanged original condition and its call to the test
         * observer, plus the single authored host-pump insertion. */
        #define ProcessInterrupts observe_process_interrupts
        CHECK_FOR_INTERRUPTS();
    }
    CHECK_FOR_INTERRUPTS();
    #undef ProcessInterrupts
    if (setitimer(ITIMER_REAL, &zero, NULL) != 0) return -3;
    if (sigaction(SIGALRM, &previous, NULL) != 0) return -4;
    return safe_fired == 1 && safe_processed == 1 && checks > 0 ? 1 : -5;
}

#ifdef DPG_NATIVE_MAIN
#include <stdio.h>
static int native_polls;
void dpg_host_poll_signals(void) { native_polls++; }
int main(void) {
    int result = dpg_fixture_safe_point_timer();
    printf("result=%d fired=%d processed=%d polls=%d\n", result, (int)safe_fired, safe_processed, native_polls);
    return result == 1 && native_polls > 0 ? 0 : 1;
}
#endif
