#include "program.h"

// This declaration is intentionally unused and must remain source metadata.
template<class T> struct Unused;
extern Unused<int> unused_declaration(Unused<int> value);

int main() {
    int* original = &imported_value;
    int (*selected)(int) = &imported_add;
    if (original != &imported_value || selected != &imported_add) return 1;
    if (selected(5) != 22 || imported_add(5) != 22) return 2;
    *original = 23;
    if (&imported_value != original || imported_value != 23) return 3;
    if (get_imported_value() != 42) return 4;
    if (&typeid(void) != provider_void_info()) return 5;
    return second_unit_check();
}
