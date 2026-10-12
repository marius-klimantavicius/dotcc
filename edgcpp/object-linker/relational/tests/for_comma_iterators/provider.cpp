#include "program.h"
const int values[4] = {2, 3, 5, 7};
int trace = 0;
void append(int digit) { trace = trace * 10 + digit; }
int check_iterators() {
    int index = 0;
    const int* cursor = values;
    int total = 0;
    for (; index < 3; ++index, ++cursor) {
        if (index == 1) continue;
        total += *cursor;
    }
    if (index != 3 || cursor - values != 3 || total != 7) return 1;
    trace = 0;
    for (int count = 0; count < 2; (append(1), (++count, append(2)))) {
        if (count == 0) continue;
    }
    if (trace != 1212) return 2;
    trace = 0;
    index = 0;
    for (; index < 5; ++index, append(index)) {
        if (index == 2) break;
    }
    if (trace != 12 || index != 2) return 3;
    return 0;
}
