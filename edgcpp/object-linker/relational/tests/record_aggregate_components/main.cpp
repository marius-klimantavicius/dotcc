#include "program.h"

int main() {
    int observed = 11;
    TestValue original{&observed, 17, 23};
    Backoff direct = from_reference(&observed, original);
    if (trace != 13 || selections != 0 || direct.owner != &observed ||
        direct.test.alias != &observed || direct.test.expected != 17 ||
        direct.test.mask != 23 || direct.tail != 29) return 1;

    original.expected = 19;
    if (direct.test.expected != 17) return 2;

    trace = 0;
    Backoff evaluated{mark_owner(1, &observed), selected(original), mark(3, 31)};
    if (trace != 123 || selections != 1 || evaluated.owner != &observed ||
        evaluated.test.alias != &observed || evaluated.test.expected != 19 ||
        evaluated.test.mask != 23 || evaluated.tail != 31) return 3;

    trace = 0;
    Backoff returned{mark_owner(1, &observed), make_test(&observed, 37, 41), mark(3, 47)};
    if (trace != 143 || selections != 1 || returned.owner != &observed ||
        returned.test.alias != &observed || returned.test.expected != 37 ||
        returned.test.mask != 41 || returned.tail != 47) return 4;

    original.mask = 43;
    observed = 53;
    if (direct.test.mask != 23 || evaluated.test.mask != 23 ||
        returned.test.mask != 41 || *direct.test.alias != 53 ||
        *evaluated.test.alias != 53 || *returned.test.alias != 53) return 5;
    return 0;
}
