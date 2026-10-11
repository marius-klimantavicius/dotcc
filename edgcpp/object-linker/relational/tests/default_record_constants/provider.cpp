#include "program.h"

Holder::Holder() {}
Holder::Holder(int chosen) : direct(chosen, chosen + 1), trailing(chosen) {}

int provider_defaults() {
    Holder first;
    if (first.direct.first != 5 || first.direct.second != 7 ||
        first.nested.pair.first != 11 || first.nested.pair.second != 13 ||
        first.nested.tail != 17 || first.partial.first != 19 ||
        first.partial.second != 0 || first.trailing != 23) return 1;
    Holder overridden(29);
    if (overridden.direct.first != 29 || overridden.direct.second != 30 ||
        overridden.nested.pair.first != 11 || overridden.nested.pair.second != 13 ||
        overridden.nested.tail != 17 || overridden.partial.first != 19 ||
        overridden.partial.second != 0 || overridden.trailing != 29) return 2;
    first.direct.first = 31;
    first.nested.pair.second = 37;
    first.partial.second = 41;
    if (overridden.direct.first != 29 || overridden.nested.pair.second != 13 ||
        overridden.partial.second != 0) return 3;
    return 0;
}
