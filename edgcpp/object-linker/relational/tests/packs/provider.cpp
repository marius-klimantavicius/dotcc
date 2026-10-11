#include "program.h"

int pack_result() {
    Wrapped<Box, int> wrapped;
    wrapped.item.value = 9;
    return wrapped.item.value + count(1, 2L, 3) + count() + sum<2, -3, 5>();
}
