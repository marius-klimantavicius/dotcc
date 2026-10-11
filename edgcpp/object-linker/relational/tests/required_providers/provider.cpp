#include "program.h"

extern "C" {
int imported_value = 17;
int imported_add(int value) noexcept { return imported_value + value; }
int get_imported_value() noexcept { return 42; }
const std::type_info* provider_void_info() noexcept { return &typeid(void); }
}
