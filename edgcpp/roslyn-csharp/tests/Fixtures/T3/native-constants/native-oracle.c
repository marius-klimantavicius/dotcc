/* Separate native reference process only; never a generated application provider. */
#include <stdio.h>

int fixture(void);
extern int observations[5];

int main(void) {
    printf("result=%d\n", fixture());
    for (int index = 0; index < 5; ++index)
        printf("observation[%d]=%d\n", index, observations[index]);
    return 0;
}
