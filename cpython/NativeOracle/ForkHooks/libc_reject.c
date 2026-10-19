/* Actual shared target service contract, independent of CPython hook code.
 * Native EPERM service emulation belongs to control.c's separate oracle. */
#include <errno.h>
#include <stdio.h>
#include <unistd.h>

int main(void)
{
    char path[] = "/must-not-execute";
    char *arguments[] = {path, NULL};
    char *environment[] = {NULL};
    for (int i = 0; i < 8; i++) {
        errno = 0;
        if (fork() != -1 || errno != EPERM) return 1;
        errno = 0;
        if (execv(path, arguments) != -1 || errno != EPERM) return 2;
        errno = 0;
        if (execvp(path, arguments) != -1 || errno != EPERM) return 3;
        errno = 0;
        if (execve(path, arguments, environment) != -1 || errno != ENOTSUP) return 4;
    }
    puts("shared-process-rejection passed=32 fork-execv-execvp=EPERM execve=ENOTSUP children=0");
    return 0;
}
