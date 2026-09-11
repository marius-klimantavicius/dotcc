#include <stdio.h>
#include <stddef.h>
#include "shared.h"
int main(void) {
    struct shared value = {0};
    value.prefix = 1;
    value.first.number = 10;
    value.first.mask = 11;
    value.named.code = 12;
    value.state = READY;
    int result = read_shared(&value);
    printf("values %d %d %u %d %d %d %d\n", value.prefix, value.second.number, value.second.mask, value.named.code, value.named.omitted, (int)value.state, result);
    printf("layout %d %d %d %d %d\n", (int)sizeof(value), (int)offsetof(struct shared, first.number), (int)offsetof(struct shared, named.code), (int)((char*)&value.second.number-(char*)&value), shared_layout());
    return 0;
}
