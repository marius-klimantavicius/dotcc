#include <assert.h>
#include <stdio.h>
typedef int digit;
static int add(register int first, register int digit) { return first+digit; }
static int call(register int (*action)(int, int), register int values[2]) {
    return action(values[0],values[1]);
}
digit after;
int main(void) { int values[2]={3,4}; after=call(add,values); assert(after==7); printf("%d\n",after); return 0; }
