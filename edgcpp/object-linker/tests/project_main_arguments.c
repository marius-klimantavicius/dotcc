#include <string.h>
#include <stdlib.h>
#include <stdio.h>
static char **saved;
static void after_main(void) {
    if (saved[0][0] != 'P' || strcmp(saved[1], "mutable") || saved[5] != 0) abort();
    puts("argv survived normal exit");
}
int main(int count, char **arguments) {
    if (count != 5 || !arguments[0] || !arguments[0][0] || arguments[5]) return 1;
    if (strcmp(arguments[1], "mutable") || strcmp(arguments[2], "") ||
        strcmp(arguments[3], "žąsis") || strcmp(arguments[4], "🦆")) return 2;
    arguments[0][0] = 'P';
    saved = arguments;
    if (atexit(after_main)) return 3;
    return 17;
}
