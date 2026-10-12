#include "program.h"

void *constructed_table_address(int input) noexcept {
    External value(input);
    return *(void **)&value;
}

int dispatch(External *value, int add) noexcept { return value->eval(add); }
