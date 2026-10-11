#include <typeinfo>

extern "C" {
extern int imported_value;
int imported_add(int value) noexcept;
int get_imported_value() noexcept;
const std::type_info* provider_void_info() noexcept;
}

int second_unit_check();
