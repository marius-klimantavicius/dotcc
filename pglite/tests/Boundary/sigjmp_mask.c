#define _POSIX_C_SOURCE 200809L
#include <setjmp.h>
#include <signal.h>
#include <stddef.h>
#include <stdio.h>
#include <sys/time.h>
/* Real lexical sigsetjmp contract; it must restore only when requested. */
int dpg_fixture_sigjmp_mask(int save) {
    sigjmp_buf target;
    sigset_t original, changed, observed;
    sigprocmask(SIG_SETMASK, NULL, &original);
    sigemptyset(&changed);
    sigaddset(&changed, SIGALRM);
    sigprocmask(SIG_UNBLOCK, &changed, NULL);
    if (sigsetjmp(target, save) == 0) {
        sigprocmask(SIG_BLOCK, &changed, NULL);
        siglongjmp(target, 7);
    }
    sigprocmask(SIG_SETMASK, NULL, &observed);
    sigprocmask(SIG_SETMASK, &original, NULL);
    return sigismember(&observed, SIGALRM);
}

static sigjmp_buf once_target;
static int buffer_evaluations, mask_evaluations, old_result, prefix_evaluations;
static void *select_buffer(int old) {
    ++buffer_evaluations;
    old_result = old;
    return once_target;
}
static int select_mask(int old) {
    ++mask_evaluations;
    return old == 42;
}
int dpg_fixture_sigjmp_once(void) {
    buffer_evaluations = mask_evaluations = 0;
    int result = 42;
    result = sigsetjmp(select_buffer(result), select_mask(result));
    if (result == 0) siglongjmp(once_target, 0);
    return result == 1 && buffer_evaluations == 1 && mask_evaluations == 1 && old_result == 42 ? 0 : 1;
}

static int select_prefix(int enabled) { ++prefix_evaluations; return enabled; }
int dpg_fixture_sigjmp_conditional(int enabled) {
    buffer_evaluations = mask_evaluations = prefix_evaluations = 0;
    if (select_prefix(enabled) && sigsetjmp(select_buffer(42), select_mask(42)) != 0)
        return prefix_evaluations == 1 && buffer_evaluations == 1 && mask_evaluations == 1 ? 42 : -1;
    if (enabled) siglongjmp(once_target, 7);
    return prefix_evaluations == 1 && buffer_evaluations == 0 && mask_evaluations == 0 ? 3 : -2;
}

int dpg_fixture_sigjmp_nested(void) {
    sigjmp_buf outer_target, inner_target;
    int outer = sigsetjmp(outer_target, 1);
    if (outer == 0) {
        int inner = sigsetjmp(inner_target, 0);
        if (inner == 0) siglongjmp(inner_target, 3);
        if (inner == 3) siglongjmp(outer_target, 7);
        return 1;
    }
    return outer == 7 ? 0 : 2;
}

int dpg_fixture_sigjmp_switch(void) {
    sigjmp_buf target;
    switch (sigsetjmp(target, 0)) {
        case 0: siglongjmp(target, -3); break;
        case -3: return 0;
        default: return 1;
    }
    return 2;
}

int dpg_fixture_sigjmp_repeated(void) {
    sigjmp_buf target;
    int result;
    if (!(result = sigsetjmp(target, 1))) siglongjmp(target, 2);
    else if (result == 2) siglongjmp(target, 3);
    return result == 3 ? 0 : 1;
}

static sigjmp_buf handler_target;
static void jumping_handler(int signal) {
    sigset_t extra;
    sigemptyset(&extra);
    sigaddset(&extra, SIGUSR1);
    sigprocmask(SIG_BLOCK, &extra, NULL);
    siglongjmp(handler_target, 0);
}
int dpg_fixture_sigjmp_handler(int save) {
    struct sigaction action, original_action;
    sigset_t original_mask, empty, observed;
    sigprocmask(SIG_SETMASK, NULL, &original_mask);
    sigemptyset(&empty);
    sigprocmask(SIG_SETMASK, &empty, NULL);
    action.sa_handler = jumping_handler;
    action.sa_flags = 0;
    sigemptyset(&action.sa_mask);
    sigaction(SIGALRM, &action, &original_action);
    int result = sigsetjmp(handler_target, save);
    if (result == 0) raise(SIGALRM);
    sigprocmask(SIG_SETMASK, NULL, &observed);
    sigaction(SIGALRM, &original_action, NULL);
    sigprocmask(SIG_SETMASK, &original_mask, NULL);
    if (result != 1) return 1;
    return sigismember(&observed, SIGALRM) == (save == 0)
        && sigismember(&observed, SIGUSR1) == (save == 0) ? 0 : 2;
}

int dpg_fixture_pending_consume(void) {
    sigset_t selected, pipe, alarm, original, pending;
    sigemptyset(&selected); sigaddset(&selected, SIGPIPE); sigaddset(&selected, SIGALRM);
    sigemptyset(&pipe); sigaddset(&pipe, SIGPIPE);
    sigemptyset(&alarm); sigaddset(&alarm, SIGALRM);
    sigprocmask(SIG_BLOCK, &selected, &original);
    raise(SIGPIPE); raise(SIGPIPE); raise(SIGALRM);
    if (sigpending(&pending) || !sigismember(&pending, SIGPIPE) || !sigismember(&pending, SIGALRM)) return 1;
    int signal = 0;
    if (sigwait(&pipe, &signal) || signal != SIGPIPE) return 2;
    if (sigpending(&pending) || sigismember(&pending, SIGPIPE) || !sigismember(&pending, SIGALRM)) return 3;
    if (sigwait(&alarm, &signal) || signal != SIGALRM) return 4;
    if (sigpending(&pending) || sigismember(&pending, SIGPIPE) || sigismember(&pending, SIGALRM)) return 5;
    sigprocmask(SIG_SETMASK, &original, NULL);
    return 0;
}

/* The caller arms its own timer before entering this translated wait. */
int dpg_fixture_pending_timer(void) {
    sigset_t alarm, original, pending;
    sigemptyset(&alarm); sigaddset(&alarm, SIGALRM);
    sigprocmask(SIG_BLOCK, &alarm, &original);
    int signal = 0;
    int result = sigwait(&alarm, &signal);
    if (result || signal != SIGALRM) return 1;
    if (sigpending(&pending) || sigismember(&pending, SIGALRM)) return 2;
    sigprocmask(SIG_SETMASK, &original, NULL);
    return 0;
}

static sigjmp_buf expired_target;
static int invalid_recovery;
void dpg_fixture_sigjmp_expire(void) {
    if (sigsetjmp(expired_target, 0) == 0) return;
    invalid_recovery++;
}
void dpg_fixture_sigjmp_stale(void) { siglongjmp(expired_target, 1); }
int dpg_fixture_sigjmp_invalid_recovery(void) { return invalid_recovery; }
void dpg_fixture_sigjmp_foreign(int save, void (*callback)(void *)) {
    sigjmp_buf target;
    if (sigsetjmp(target, save) == 0) callback(target);
    else invalid_recovery++;
}

#ifdef DPG_NATIVE_MAIN
int main(void) {
    if (dpg_fixture_sigjmp_mask(0) != 1 || dpg_fixture_sigjmp_mask(1) != 0) return 1;
    if (dpg_fixture_sigjmp_once()) return 2;
    if (dpg_fixture_sigjmp_conditional(0) != 3 || dpg_fixture_sigjmp_conditional(1) != 42) return 9;
    if (dpg_fixture_sigjmp_nested()) return 3;
    if (dpg_fixture_sigjmp_switch()) return 4;
    if (dpg_fixture_sigjmp_repeated()) return 5;
    if (dpg_fixture_sigjmp_handler(0) || dpg_fixture_sigjmp_handler(1)) return 6;
    if (dpg_fixture_pending_consume()) return 7;
    struct itimerval timer = { { 0, 0 }, { 0, 2000 } };
    if (setitimer(ITIMER_REAL, &timer, NULL) || dpg_fixture_pending_timer()) return 8;
    puts("sigjmp: masks, operands, nested, switch, repeated, handler passed");
    puts("signals: blocked pending selection and timer consumption passed");
    return 0;
}
#endif
