#include "program.h"

int main() {
    trace = 0;
    if (exercise(false) != 11) return 1;
    // Prior effect, outer construction, ordered members, body, Pair destructor,
    // members and outer destruction on the lexical return path.
    if (trace != 931234768) return 2;
    trace = 0;
    try {
        exercise(true);
        return 3;
    } catch (Failure &failure) {
        // The inner reference handler changed the original exception payload.
        if (failure.code != 8) return 4;
        mark(failure.code);
    }
    // The second member threw: only the first completed member is destroyed;
    // Then the inner handler, outer Guard, rethrow handler and exception
    // payload destructor execute.
    if (trace != 931267889) return 5;
    return 0;
}
