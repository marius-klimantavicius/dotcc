/* Explicit OS process-boundary control, never a PostgreSQL backend. */
#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>

int main(int argc, char **argv)
{
    const char *mode = getenv("DOTCC_LOCALE_PROCESS_CONTROL");
    if (argc != 2 || strcmp(argv[1], "-a") != 0 || mode == NULL)
        return 91;
    if (strcmp(mode, "signal") == 0)
        raise(SIGTERM);
    if (strcmp(mode, "block") == 0)
    {
        printf("%ld\n", (long)getpid());
        fflush(stdout);
        for (;;) pause();
    }
    /* stderr exceeds a normal pipe's capacity before stdout is produced. */
    for (int i = 0; i < 100000; i++) fputc('e', stderr);
    fwrite("a\0z\n", 1, 4, stdout);
    return 7;
}
