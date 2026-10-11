#include "program.h"

long long trace = 0;
int alive = 0;
int constructions = 0;
int completions = 0;
bool throw_holder = false;

Guard::Guard(int value) : id(value) {
    trace = trace * 10 + value;
    if (value == 4 && throw_holder) throw 41;
    ++constructions;
    ++alive;
}
Guard::~Guard() { trace = trace * 10 + id + 5; --alive; }
void reset() { trace = 0; alive = 0; constructions = 0; completions = 0; throw_holder = false; }

int lookup(bool found) {
    Guard outer(1);
    int result = 0;
    if (found) {
        Guard branch(2);
        for (int index = 0; index < 2; ++index) {
            Guard iteration(3);
            if (index == 1) {
                result = 9;
                goto __done;
            }
        }
    }
    {
        Guard holder(4);
        result = 7;
    }
__done:
    ++completions;
    return result;
}

int same_scope(bool early) {
    Guard outer(1);
    if (early) goto finished;
    { Guard holder(4); }
finished:
    ++completions;
    return 5;
}
