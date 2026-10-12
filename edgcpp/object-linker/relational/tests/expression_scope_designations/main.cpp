#include "program.h"
void reset() {
    trace = 0;
    cleaned = 0;
    accumulator.value = 0;
    fail_receiver = false;
    fail_call = false;
    fail_payload = false;
}
int main() {
    reset();
    discard_record(false);
    if (trace != 1234 || cleaned != 1 || accumulator.value != 107) return 1;
    reset();
    Accumulator *saved = scoped_address();
    if (saved != &accumulator || saved->value != 107 || trace != 1234 || cleaned != 1) return 2;
    reset();
    try { discard_record(true); return 3; }
    catch (int error) { if (error != 7 || trace != 1 || cleaned != 0) return 4; }
    reset(); fail_receiver = true;
    try { discard_record(false); return 5; }
    catch (int error) { if (error != 8 || trace != 124 || cleaned != 1) return 6; }
    reset(); fail_call = true;
    try { discard_record(false); return 7; }
    catch (int error) { if (error != 9 || trace != 1234 || cleaned != 1) return 8; }
    reset();
    try { throw_scoped(false); return 9; }
    catch (const Error &error) { if (error.value != 17 || trace != 154 || cleaned != 1) return 10; }
    reset();
    try { throw_scoped(true); return 11; }
    catch (int error) { if (error != 7 || trace != 1 || cleaned != 0) return 12; }
    reset(); fail_payload = true;
    try { throw_scoped(false); return 13; }
    catch (int error) { if (error != 10 || trace != 154 || cleaned != 1) return 14; }
    return 0;
}
