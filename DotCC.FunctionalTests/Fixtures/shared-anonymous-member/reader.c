#include <stddef.h>
struct unrelated { struct { int padding; } member; };
#include "shared.h"
int read_shared(struct shared *value) {
    int result = value->first.number + (int)value->first.mask + value->named.code + READY;
    value->second.number += 2;
    value->state = DONE;
    return result;
}
int shared_layout(void) { return sizeof(struct shared) + offsetof(struct shared, second.number) + offsetof(struct shared, named.code); }
