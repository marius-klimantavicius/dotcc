#include <stdio.h>
#include <stddef.h>
#include "types.h"
int (globals[3])={3,5,7};
static int (scalar)=13;
int main(void) {
    int (locals)[3]={2,4,6};
    int (first)=17, ((second))=19;
    struct slots value={0};
    value.values[2]=&globals[2]; value.rows[1][2]=locals[2]; value.scalar=scalar;
    printf("%d %d %lu %lu %lu\n",sum_slots(&value),first+second,
        (unsigned long)sizeof(value.values),(unsigned long)sizeof(value.rows),
        (unsigned long)offsetof(struct slots,scalar));
    return 0;
}
