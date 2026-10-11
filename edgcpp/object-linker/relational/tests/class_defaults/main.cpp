#include "program.h"
int main() {
    { Defaulted value(0); if (alive != 2 || value.first.value != 1 || value.second.value != 2) return 1; }
    if (alive != 0 || destroyed != 21) return 2;
    destroyed = 0;
    try { Defaulted failed(1); return 3; } catch (int error) { if (error != 17 || alive != 0 || destroyed != 21) return 4; }
    destroyed = 0;
    { Defaulted explicit_value; if (alive != 2 || explicit_value.first.value != 2) return 5; }
    if (alive != 0 || destroyed != 22) return 6;
    destroyed = 0;
    try { ExitFailure failed(false); return 7; } catch (int error) { if (error != 31 || alive != 0 || destroyed != 1) return 8; }
    destroyed = 0;
    try { ExitFailure failed(true); return 9; } catch (int error) { if (error != 31 || alive != 0 || destroyed != 1) return 10; }
    return 0;
}
