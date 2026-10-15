#include <stdio.h>
extern int fixture(void), control_observation_count(void), control_observe(int);
int main(void) {
    printf("result=%d\n", fixture());
    printf("observations=%d\n", control_observation_count());
    for (int index = 0; index < control_observation_count(); ++index)
        printf("%d:%d\n", index, control_observe(index));
    return 0;
}
