#include <stdio.h>

static int increment(int value) { return value + 1; }

static int invoke(int (*const callback)(int), int value) {
    return callback(value);
}

static int invoke_slot(int (*const *slot)(int), int value) {
    return (*slot)(value);
}

int main(void) {
    int (*const callback)(int) = increment;
    printf("parameters %d %d\n", invoke(callback, 41), invoke_slot(&callback, 40));
    return 0;
}
