#include <stdio.h>
#include <stdlib.h>
typedef int (*Callback)(int);
int increment(int);
Callback from_other(void);
Callback runtime_other(void);
static Callback table[] = { increment, &increment, 0 };
static int local(int value) { return value + 3; }
Callback local_other(void);
int main(void) {
    Callback first = increment;
    Callback runtime = abs;
    int valid = 1;
    for (int i = 0; i < 30000; i++) {
        valid = valid && first == table[0] && first == table[1] && first == from_other();
        valid = valid && runtime == runtime_other() && runtime(-42) == first(41);
        valid = valid && (void*)first == (void*)increment;
    }
    printf("%d %d %d %d\n", valid, first(41), table[2] == 0, (void*)(Callback)-1 == (void*)-1);
    printf("%d %d %d\n", local(1), local_other()(1), local_other() != local);
    return 0;
}
