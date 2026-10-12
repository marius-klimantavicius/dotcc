#include "program.h"
int main() {
    trace = 0; arguments = 0;
    Value first(7);
    if (first.self != &first || first.value != 7 || trace != 12 || arguments != 1) return 1;
    trace = 0; arguments = 0;
    Value nested(8, false);
    if (nested.self != &nested || nested.value != 8 || trace != 123 || arguments != 1) return 2;
    trace = 0; arguments = 0;
    try { Value failed(-1); return 3; }
    catch (int error) { if (error != 31 || trace != 1 || arguments != 1) return 4; }
    trace = 0; arguments = 0;
    try { Value failed(-2); return 5; }
    catch (int error) { if (error != 34 || trace != 0 || arguments != 1) return 6; }
    trace = 0; arguments = 0;
    try { Value failed(99); return 7; }
    catch (int error) { if (error != 32 || trace != 12 || arguments != 1) return 8; }
    trace = 0; arguments = 0;
    try { Value failed(8, true); return 9; }
    catch (int error) { if (error != 33 || trace != 123 || arguments != 1) return 10; }
    return 0;
}
