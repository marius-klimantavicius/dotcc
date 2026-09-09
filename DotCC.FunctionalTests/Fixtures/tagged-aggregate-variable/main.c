#include <stdio.h>
static struct Counters { int first; int second; } counters = {10, 20};
static union Number { int value; unsigned int bits; } number = {12};
static struct State { int count; } state;
int main(void) {
    static const struct Entry { const char *name; int value; } entries[] = {{"x", 7}, {"y", 8}};
    struct Local { int value; } local = {9};
    state.count = entries[1].value;
    printf("%d %d %d %d\n", counters.first + counters.second + number.value, state.count, local.value, entries[0].value);
    return 0;
}
