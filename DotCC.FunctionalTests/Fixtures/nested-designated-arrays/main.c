/* Array-entry designators preserve target type, ordering, and omitted zeroes. */
#include <stdio.h>
typedef struct Entry { int value; int extra; const char *name; } Entry;
static Entry globals[] = {
    {.name = "alpha", .value = 11},
    {.extra = 22, .value = 2, .value = 33},
};
struct Outer { Entry inner; int tail; };

static void local_static(void) {
    static Entry entries[] = {{.name = "local", .extra = 7}, {.value = 8}};
    entries[1].value++;
    printf("%s %d %d %d\n", entries[0].name, entries[0].value,
           entries[0].extra, entries[1].value);
}

int main(void) {
    Entry locals[3] = {{.extra = 44, .value = 55}, {.name = "beta"}};
    struct Outer nested[] = {{{.value = 66, .name = "gamma"}, 77}};
    printf("%d %d %s %d %d\n", globals[0].value, globals[0].extra,
           globals[0].name, globals[1].value, globals[1].extra);
    printf("%d %d %s %d\n", locals[0].value, locals[0].extra,
           locals[1].name, locals[2].value);
    printf("%d %d %s %d\n", nested[0].inner.value, nested[0].inner.extra,
           nested[0].inner.name, nested[0].tail);
    local_static();
    local_static();
    return 0;
}
