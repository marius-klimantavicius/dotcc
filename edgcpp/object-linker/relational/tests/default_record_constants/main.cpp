#include "program.h"

int main() {
    if (provider_defaults()) return 1;
    Holder value;
    if (value.direct.first != 5 || value.direct.second != 7 ||
        value.nested.pair.first != 11 || value.nested.pair.second != 13 ||
        value.nested.tail != 17 || value.partial.first != 19 ||
        value.partial.second != 0 || value.trailing != 23) return 2;
    return 0;
}
