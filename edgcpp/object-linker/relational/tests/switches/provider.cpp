#include "program.h"
int alive = 0;
Watch::Watch(int input) : value(input) { if (input < 0) throw 17; ++alive; }
Watch::~Watch() { --alive; }
Watch::operator int() const { return value; }
int regular(int input) {
    int result = 0;
    switch (input) {
    case 0: result = 1;
    case 1: result += 2; break;
    default: result = 20;
    case 5: result += 5; break;
    case 7: case 8: result = 70; break;
    }
    return result;
}
int controlled(int input) {
    int result = 0;
    switch (Watch outer{9}; Watch selected{input}) {
    case 0: if (alive != 2) return -1; result = 10; break;
    case 1: { Watch local{3}; if (alive != 3) return -2; result = 20; break; }
    case 2: { Watch local{4}; if (alive != 3) return -3; return 55; }
    case 3: throw 23;
    default: result = 40; break;
    }
    if (alive != 0) return -4;
    return result;
}
int nested() {
    int count = 0;
    for (int index = 0; index < 4; ++index) {
        switch (index) {
        case 0: continue;
        case 1:
            for (int inner = 0; inner < 3; ++inner) { ++count; if (inner == 1) break; }
            break;
        case 2:
            switch (count) { case 12: count += 3; break; default: return -1; }
            break;
        default: ++count; break;
        }
        count += 10;
    }
    return count;
}
int wide(unsigned long long input) {
    switch (input) { case 18446744073709551615ULL: return 1; case 0: return 2; default: return 3; }
}
int skipped(int input) {
    int result = 5;
    switch (input) { case 0: result = 7; break; }
    switch (input) { int scratch; case 0: scratch = 3; result += scratch; break; default: break; }
    return result;
}
