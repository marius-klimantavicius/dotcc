/* Reduced mixed brace lists in a declaration list, as rejected in pylifecycle. */
#include <stdio.h>
struct entry { int first, second, third; };
int main(void) {
    struct entry a={1,.second=2}, b={.first=3,4};
    printf("%d %d %d %d %d %d\n",a.first,a.second,a.third,b.first,b.second,b.third);
    return 0;
}
