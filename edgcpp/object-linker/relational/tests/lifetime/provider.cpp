#include "program.h"

int trace = 0;
void mark(int value) { trace = trace * 10 + value; }

Failure::Failure(int value) : code(value) {}
Failure::~Failure() { mark(9); }

Guard::Guard(int input, bool fail) : value(input) {
    mark(value);
    if (fail) throw Failure(7);
}

Guard::~Guard() { mark(value + 5); }

Pair::Pair(bool fail) : first(1, false), second(2, fail) { mark(3); }
Pair::~Pair() { mark(4); }

int exercise(bool fail) {
    mark(9);
    Guard outside(3, false);
    try {
        Pair pair(fail);
        if (!fail) return 11;
    } catch (int &unexpected) {
        mark(unexpected);
        return 99;
    } catch (Failure &failure) {
        mark(failure.code);
        failure.code = 8;
        throw;
    }
    return 12;
}
