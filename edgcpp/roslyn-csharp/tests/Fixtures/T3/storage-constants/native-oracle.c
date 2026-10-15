/* Independent native reference process only; never a generated dependency. */
#include <stdio.h>
extern int fixture(void);
extern int storage_observation_count(void);
extern int storage_observe(int);
int main(void) {
    for (int iteration = 0; iteration < 2; ++iteration) {
        printf("result=%d\n", fixture());
        for (int index = 0; index < storage_observation_count(); ++index)
            printf("%d:%d\n", index, storage_observe(index));
    }
    return 0;
}
