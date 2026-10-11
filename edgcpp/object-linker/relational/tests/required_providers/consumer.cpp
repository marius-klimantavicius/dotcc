#include "program.h"

int second_unit_check() {
    int* first = &imported_value;
    int* second = &imported_value;
    int (*selected)(int) = &imported_add;
    return first == second && selected == &imported_add && selected(3) == 26 &&
        &typeid(void) == provider_void_info() ? 0 : 1;
}
