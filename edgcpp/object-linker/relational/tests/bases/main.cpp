#include "program.h"

int add_right(Right *value, int amount) {
    value->right += amount;
    return value->right;
}

int main() {
    trace = 0;
    if (exercise(false) != 0) return 1;
    // Left, Right, Derived body; Derived destructor, Right, Left.
    if (trace != 123476) return 2;
    trace = 0;
    try {
        exercise(true);
        return 3;
    } catch (int &failure) {
        if (failure != 7) return 4;
        mark(9);
    }
    // A failed Right constructor destroys only the completed Left base.
    if (trace != 1269) return 5;
    return 0;
}
