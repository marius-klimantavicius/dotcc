/* Selected policy and actual shared services. Core layouts are measured by the
   separate header control and, after generation, the actual product consumer. */
#include "pyconfig.h"
#include <stddef.h>
#include <stdio.h>
#include <time.h>
#include <errno.h>
#include <unistd.h>
#ifdef IMPORT_NATIVE_ORACLE
/* Target errno is managed storage; native glibc errno is TLS. Read the oracle's
   actual errno without supplying any replacement service or header layout. */
extern int *__errno_location(void);
#define IMPORT_ERRNO (*__errno_location())
#else
#define IMPORT_ERRNO errno
#endif
#ifdef HAVE_SYS_WAIT_H
#include <sys/wait.h>
#endif

int main(void)
{
    int failures = 0;
    int identity_selected = 0;
#ifdef HAVE_GETPID
    int (*identity)(void) = getpid;
    int previous_errno = IMPORT_ERRNO;
    int first_pid = identity();
    identity_selected = 1;
    if (first_pid <= 0 || identity() != first_pid || IMPORT_ERRNO != previous_errno) failures++;
#endif
    int formatter_selected = 0;
    int wait_selected = 0;
    size_t formatted = 0;
#ifdef HAVE_WCSFTIME
#error The selected target has no wcsftime service contract
#endif
#ifdef HAVE_STRFTIME
    time_t epoch = 0;
    const struct tm *calendar = gmtime(&epoch);
    char output[32];
    size_t (*formatter)(char *, size_t, const char *, const struct tm *) = strftime;
    formatter_selected = 1;
    if (!calendar) return 21;
    formatted = formatter(output, 32, "%Y-%m-%d %H:%M:%S", calendar);
    if (formatted != 19 || output[19] != 0) failures++;
#endif
#if defined(HAVE_WAITPID) != defined(HAVE_SYS_WAIT_H)
#error The selected wait import contract must be complete
#endif
#ifdef HAVE_WAITPID
    int status = 0x12345678;
    int (*waiter)(int, int *, int) = waitpid;
    wait_selected = 1;
    if (waiter(-1, &status, WNOHANG) != -1 || status != 0x12345678) failures++;
#ifdef IMPORT_NATIVE_ORACLE
    if (IMPORT_ERRNO != ECHILD) failures++;
#else
    if (IMPORT_ERRNO != EPERM) failures++;
#endif
    if (!WIFEXITED(0x2a00) || WEXITSTATUS(0x2a00) != 42) failures++;
    if (!WIFSIGNALED(9) || WTERMSIG(9) != 9) failures++;
    if (!WIFSTOPPED(0x137f) || WSTOPSIG(0x137f) != 19) failures++;
    if (WIFSIGNALED(0x137f) || WIFSIGNALED(0x7f) || WIFSIGNALED(0xffff)) failures++;
    if (WNOHANG != 1 || WUNTRACED != 2) failures++;
#endif
    if (sizeof(size_t) != 8 || sizeof(long) != 8 || sizeof(wchar_t) != 2) failures++;
    if ((wchar_t)-1 < 0) failures++;
    printf("features %d %d %lu failures %d\n", formatter_selected, wait_selected,
           (unsigned long)formatted, failures);
    printf("identity %d failures %d\n", identity_selected, failures);
    return failures ? 22 : 0;
}
