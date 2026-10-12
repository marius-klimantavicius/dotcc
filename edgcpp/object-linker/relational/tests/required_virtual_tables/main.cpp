#include "program.h"

int main() {
    External first(19);
    External second(7);
    void *expected = provided_table_address();
    if (*(void **)&first != expected || *(void **)&second != expected) return 1;
    if (constructed_table_address(3) != expected || constructed_table_address(9) != expected) return 2;
    if (provided_table_address() != expected) return 3;
    if (dispatch(&first, 23) != 42 || dispatch(&second, 5) != 12) return 4;
    first.stored = 31;
    if (dispatch(&first, 2) != 33) return 5;
    return 0;
}
