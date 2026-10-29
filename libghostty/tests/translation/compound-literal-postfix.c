#include <stdio.h>

struct Row { unsigned long array[3]; };
struct Scalar { int value; };

int main(void) {
    int index = 1;
    unsigned long value = (struct Row){{40, 42, 44}}.array[index];
    int named = (struct Scalar){.value = 7}.value;
    printf("postfix %lu %d\n", value, named);
    return 0;
}
