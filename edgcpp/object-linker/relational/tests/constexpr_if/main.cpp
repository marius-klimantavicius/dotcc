#include "program.h"

int main() {
    if (choose<true>() != 1 || choose<false>() != 2 || trace != 12 || alive) return 1;
    trace = 0;
    if (initialized<true>(3) != 5 || trace != 354 || alive) return 2;
    trace = 0;
    if (initialized<false>(6) != 8 || trace != 687 || alive) return 3;
    trace = 0;
    try {
        throwing<true>(3);
        return 4;
    } catch (int error) {
        if (error != 17 || trace != 394 || alive) return 5;
    }
    trace = 0;
    if (throwing<false>(6) != 8 || trace != 687 || alive) return 6;
    return 0;
}
