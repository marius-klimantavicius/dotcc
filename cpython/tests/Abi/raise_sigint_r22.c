/* The required _testcapi.raise_SIGINT_then_send_None calls raise() immediately
   before entering the generator. POSIX raise must finish its installed handler
   before returning; a later Console callback cannot satisfy that ordering. */
#include <signal.h>
#include <stdio.h>

static volatile sig_atomic_t delivered;
static void handler(int signal_number)
{
    if (signal_number == SIGINT) ++delivered;
}

int main(void)
{
    signal(SIGINT, handler);
    int status = raise(SIGINT);
    int immediate = delivered;
    printf("raise SIGINT status=%d delivered-before-return=%d\n", status, immediate);
    fflush(stdout);
    return status == 0 && immediate == 1 ? 0 : 39;
}
