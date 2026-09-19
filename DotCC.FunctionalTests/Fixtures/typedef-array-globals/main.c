#include <stdio.h>
#include "types.h"
int main(void) {
    printf("%zu %u %u %u %u\n", sizeof(compound_id), (unsigned)compound_id[0],
           (unsigned)compound_id[1], (unsigned)compound_id[2], (unsigned)compound_id[15]);
    printf("%zu %zu %d %d %d\n", sizeof(cells), sizeof(cells[0]), cells[0][2], cells[1][0], mutate());
    return 0;
}
