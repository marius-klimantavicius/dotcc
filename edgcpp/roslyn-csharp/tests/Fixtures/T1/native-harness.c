#include <stdio.h>
extern int fixture(void);
int main(void) {
    printf("%d\n", fixture());
    return 0;
}
