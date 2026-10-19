#include <errno.h>
#include <stdio.h>
#include <sys/wait.h>

int main(void)
{
    int failures = 0, status = 0x12345678;
    int actual, observed_errno;
    const int values[] = {0, 42 << 8, 15, 15 | 0x80, (19 << 8) | 0x7f, 0x7f, 0xffff};
    const int exited[] = {1, 1, 0, 0, 0, 0, 0};
    const int signaled[] = {0, 0, 1, 1, 0, 0, 0};
    const int stopped[] = {0, 0, 0, 0, 1, 1, 0};
    int i;
    for (i = 0; i < 7; ++i) {
        int e = !!WIFEXITED(values[i]);
        int s = !!WIFSIGNALED(values[i]);
        int t = !!WIFSTOPPED(values[i]);
        printf("status=%d exited=%d signaled=%d stopped=%d\n", values[i], e, s, t);
        if (e != exited[i] || s != signaled[i] || t != stopped[i]) ++failures;
    }
    if (WEXITSTATUS(42 << 8) != 42 || WTERMSIG(15 | 0x80) != 15 ||
        WSTOPSIG((19 << 8) | 0x7f) != 19 || WNOHANG != 1) ++failures;
    errno = 0;
    actual = waitpid(-1, &status, WNOHANG);
    observed_errno = errno;
    /* Native oracle has no children; managed process APIs explicitly reject. */
#ifdef DOTCC_MANAGED_REJECTION
    if (actual != -1 || observed_errno != EPERM) ++failures;
#else
    if (actual != -1 || observed_errno != ECHILD) ++failures;
#endif
    if (status != 0x12345678) ++failures;
    errno = 0;
    actual = wait(&status);
    observed_errno = errno;
#ifdef DOTCC_MANAGED_REJECTION
    if (actual != -1 || observed_errno != EPERM) ++failures;
#else
    if (actual != -1 || observed_errno != ECHILD) ++failures;
#endif
    if (status != 0x12345678) ++failures;
    printf("wait rejection=-1 errno=%d status=%d failures=%d\n", observed_errno, status, failures);
    return failures ? 24 : 0;
}
