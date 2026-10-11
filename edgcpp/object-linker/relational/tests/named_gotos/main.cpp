#include "program.h"

int main() {
    reset();
    throw_holder = true;
    if (lookup(true) != 9 || trace != 12383876 || alive != 0 || constructions != 4 || completions != 1) return 1;
    reset();
    if (lookup(false) != 7 || trace != 1496 || alive != 0 || constructions != 2 || completions != 1) return 2;
    reset();
    throw_holder = true;
    try { lookup(false); return 3; }
    catch (int value) {
        if (value != 41 || trace != 146 || alive != 0 || constructions != 1 || completions != 0) return 4;
    }
    reset();
    if (same_scope(true) != 5 || trace != 16 || alive != 0 || constructions != 1 || completions != 1) return 5;
    reset();
    if (same_scope(false) != 5 || trace != 1496 || alive != 0 || constructions != 2 || completions != 1) return 6;
    return 0;
}
