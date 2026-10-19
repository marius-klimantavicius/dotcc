/* Actual selected CPython/shared headers. No mirrored interpreter structures. */
#define Py_BUILD_CORE
#include <Python.h>
#include "pycore_runtime.h"
#include <stddef.h>
#include <stdio.h>
#include <time.h>
#include <errno.h>
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
    printf("layout %lu %lu %lu %lu %lu %lu %lu %lu\n",
           (unsigned long)sizeof(PyInterpreterState),
           (unsigned long)offsetof(PyInterpreterState, warnings),
           (unsigned long)sizeof(_PyRuntimeState),
           (unsigned long)offsetof(_PyRuntimeState, _main_interpreter),
           (unsigned long)sizeof(wchar_t),
#ifdef HAVE_FORK
           (unsigned long)offsetof(PyInterpreterState, before_forkers),
           (unsigned long)offsetof(PyInterpreterState, after_forkers_parent),
           (unsigned long)offsetof(PyInterpreterState, after_forkers_child)
#else
           0UL, 0UL, 0UL
#endif
    );
    return failures ? 22 : 0;
}
