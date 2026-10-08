#include "fixture.h"

static int helper(int value) { return value * 2; }

int from_b(int value) {
    int sum = 0;
    int index = 0;
    while (index < value) {
        sum = sum + helper(index);
        index = index + 1;
    }
    return sum;
}

int main() {
    int first = from_a(3);
    int second = from_b(4);
    if (first != 12) return 1;
    if (second != 12) return 2;
    if (bump(5) != 6) return 3;
    return 0;
}
