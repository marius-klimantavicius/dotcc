#include <stdio.h>
static const int one = 1;
#define LITTLE (*(char *)&one == 1)
#define BIG (*(char *)&one == 0)
#define X(n) do_call(n, 5)
static int do_call(int n, int add) { return n + add; }
int helper(void);
int main(void) {
    int n=3, x=X(n++);
    printf("little=%d big=%d value=%d next=%d helper=%d\n", LITTLE, BIG, x, n, helper());
    return 0;
}
