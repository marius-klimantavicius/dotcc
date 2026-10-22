/* Reduced from QuickJS JSString's zero-length union tail. */
#include <stdio.h>
#include <stdlib.h>

typedef struct String {
    int wide;
    union {
        unsigned char bytes[0];
        unsigned short words[0];
    } u;
} String;

static int get(String *s, int index) {
    return s->wide ? s->u.words[index] : s->u.bytes[index];
}

int main(void) {
    String *s = (String *)malloc(sizeof(String) + 8);
    if (!s) return 2;
    s->wide = 0;
    s->u.bytes[0] = 41;
    s->u.bytes[1] = 42;
    int narrow = get(s, 1);
    s->wide = 1;
    s->u.words[0] = 1024;
    s->u.words[1] = 4097;
    int wide = get(s, 1);
    printf("flexible union: %d %d\n", narrow, wide);
    free(s);
    return narrow != 42 || wide != 4097;
}
