#include <stdio.h>
#include <stddef.h>
#include "shared.h"
int main(void) {
    struct table *p = &exported;
    printf("%zu %zu %zu %s %s %u %d %d\n", sizeof(struct table), sizeof(exported), offsetof(struct table, entries), p->format, p->entries[1].name, p->entries[0].value, p->entries[2].name == 0, read_static());
    p->entries[1].value += 3;
    printf("%u %u\n", exported.entries[1].value, exported.entries[0].mask);
    return 0;
}
