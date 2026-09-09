#include <stdio.h>
#define KIND(x) _Generic((x), void *: 1, const void *: 2, int *: 3, const int *: 4, default: 0)
typedef int (*Callback)(int);
static int values[3] = {42, 17, 9};
static int *global = 1 ? values : 0;
static int conditions, arms;
static int choose(int value) { conditions++; return value; }
static int *touch(int index) { arms += index; return &values[index]; }
static int plus(int value) { return value + 1; }
static int minus(int value) { return value - 1; }
static int read(int *value) { return value ? *value : -1; }
static int invoke(Callback callback) { return callback ? callback(41) : -1; }
static int *select_pointer(int flag, int *value) { return flag ? value : 0; }
int main(void) {
    int *pointer = values;
    const int *constant = values;
    void *erased = values;
    const void *constant_erased = values;
    Callback callback = plus;
    Callback selected = choose(1) ? plus : (void *)0;
    Callback empty = choose(0) ? callback : (1 - 1);
    printf("%d %d %d %d %d\n", KIND(1 ? pointer : erased), KIND(1 ? erased : pointer),
        KIND(1 ? pointer : constant), KIND(1 ? pointer : (void *)0), KIND(1 ? pointer : constant_erased));
    printf("%d %d %d %d\n", read(choose(1) ? pointer : 0), read(choose(0) ? 0L : pointer),
        read((int *)(choose(1) ? erased : 0)), read(select_pointer(0, pointer)));
    printf("%d %d %d %d %d\n", invoke(selected), invoke(empty), invoke(choose(0) ? plus : minus),
        invoke(choose(1) ? callback : 0), global == values);
    conditions = 0; arms = 0;
    pointer = choose(0) ? touch(1) : (choose(1) ? touch(2) : 0);
    printf("%d %d %d\n", *pointer, conditions, arms);
    conditions = 0; arms = 0;
    pointer = choose(0) ? (arms++, touch(1)) : (void *)0;
    printf("%d %d %d %d\n", pointer == 0, conditions, arms, (int)sizeof(1 ? values : 0));
    return 0;
}
