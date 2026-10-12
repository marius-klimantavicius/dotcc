#include "program.h"

int External::eval(int add) noexcept { return stored + add; }

extern "C" void *provided_table_address() noexcept {
    External value(0);
    return *(void **)&value;
}
