#include <stdio.h>
static int shadow(void) { int stdout = 37; return stdout; }
int main(void) {
    FILE *out = 1 ? stdout : fopen("unused-stream-file", "w");
    FILE *err = 0 ? out : stderr;
    FILE *in = 1 ? stdin : (FILE*)0;
    fprintf(out, "%lu %lu %lu %d %d %d %d\n",
        (unsigned long)sizeof(stdin), (unsigned long)sizeof(stdout),
        (unsigned long)sizeof(stderr), out == stdout, err == stderr,
        in == stdin, shadow());
    return 0;
}
