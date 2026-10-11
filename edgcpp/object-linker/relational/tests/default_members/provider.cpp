#include "program.h"
int provider_defaults() {
    Defaults value;
    if (value.value != 7 || value.pointer || !value.enabled || value.count != -9 || value.fraction != 1.25 || value.mode != Mode::Ready) return 1;
    Defaults explicit_value(23);
    if (explicit_value.value != 23 || explicit_value.pointer || explicit_value.count != -9) return 2;
    Inherited derived;
    if (derived.value != 17 || derived.own != 5 || derived.fraction != 1.25) return 3;
    Number<31> selected;
    return selected.value == 31 ? 0 : 4;
}
