#include "program.h"
int main() {
    if (regular(0) != 3 || regular(1) != 2 || regular(5) != 5 || regular(6) != 25 || regular(7) != 70 || regular(8) != 70) return 1;
    if (controlled(0) != 10 || controlled(1) != 20 || controlled(2) != 55 || controlled(4) != 40 || alive != 0) return 2;
    try { controlled(3); return 3; } catch (int error) { if (error != 23 || alive != 0) return 4; }
    try { controlled(-1); return 5; } catch (int error) { if (error != 17 || alive != 0) return 6; }
    if (nested() != 36) return 7;
    if (wide(18446744073709551615ULL) != 1 || wide(0) != 2 || wide(4) != 3) return 8;
    if (skipped(0) != 10 || skipped(1) != 5) return 9;
    return 0;
}
