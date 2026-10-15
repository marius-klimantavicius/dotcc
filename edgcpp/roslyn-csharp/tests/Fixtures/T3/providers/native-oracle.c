/* Separate native reference process only; never a generated dependency. */
#include <stdio.h>
extern int fixture(void);
extern int provider_observation_count(void);
extern int provider_observe(int selector);
int main(void) {
    printf("result=%d\n", fixture());
    int count = provider_observation_count();
    printf("observations=%d\n", count);
    for (int index = 0; index < count; ++index)
        printf("%d:%d\n", index, provider_observe(index));
    return 0;
}
