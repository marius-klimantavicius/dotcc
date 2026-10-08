#include "cpp_linked_indirect.h"
int main() {
    Value first = callbacks.make(7);
    if (first.value != 7 || copies != 0) return 1;
    Value second = first.successor(4);
    if (first.value != 7 || second.value != 11 || copies != 0) return 2;
    if (callbacks.consume(first) != 108 || first.value != 7 || copies != 1) return 3;
    if (consume_value(second) != 112 || second.value != 11 || copies != 2) return 4;
    // Parameter copy and return copy are both required. The parameter is not
    // an NRVO candidate; no test expectation depends on optional elision.
    Value bounced = callbacks.bounce(first);
    if (bounced.value != 9 || first.value != 7 || copies != 4) return 5;
    Value advanced = add_value(second, 5);
    if (advanced.value != 17 || second.value != 11 || copies != 5) return 6;
    // An ordinary source pointer is never a result-buffer argument merely
    // because a lowered factory happens to have the same physical signature.
    callbacks.first(&first, 23);
    callbacks.last(29, &second);
    if (first.value != 23 || second.value != 29 || copies != 5) return 7;
    Value values[2] = {callbacks.make(31), make_value(37)};
    if (values[0].value != 31 || values[1].value != 37 || copies != 5) return 8;
    Value nested = callbacks.bounce(callbacks.make(41));
    if (nested.value != 42 || copies != 6) return 9;
    // The array default-argument adapter calls a constructor whose written
    // value parameter is physically indirect. Complete/subobject entries must
    // forward the same source ordinal without adding another copy.
    Holder holders[2];
    if (holders[0].observed != 3 || holders[1].observed != 3 || copies != 6) return 10;
    Holder explicit_holder(first);
    if (explicit_holder.observed != 24 || first.value != 23 || copies != 7) return 11;
    return 0;
}
