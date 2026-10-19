#define _GNU_SOURCE
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>

int main(void)
{
    char *original = getcwd(NULL, 0);
    char temporary[] = "/tmp/dotcc-b28-cwd-XXXXXX";
    if (!original || !mkdtemp(temporary) || chdir(temporary) || rmdir(temporary)) return 21;
    char buffer[4096];
    memset(buffer, 0xa5, sizeof(buffer));
    errno = 123;
    char *result = getcwd(buffer, sizeof(buffer));
    int observed = errno;
    int failed = result != NULL || observed != ENOENT;
    printf("deleted-cwd null=%d errno=%d expected=%d\n", result == NULL, observed, ENOENT);
    if (chdir(original)) return 22;
    free(original);
    char *restored = getcwd(buffer, sizeof(buffer));
    if (!restored || restored[0] != '/') failed = 1;
    printf("restored-cwd absolute=%d\n", restored && restored[0] == '/');
    return failed ? 23 : 0;
}
