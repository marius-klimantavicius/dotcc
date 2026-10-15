#include <stdio.h>
extern int fixture(void);
extern int memory_width_count(void);
extern long long memory_width_observe(int);
int main(void) {
    printf("result=%d\n", fixture());
    printf("observations=%d\n", memory_width_count());
    for (int index = 0; index < memory_width_count(); ++index)
        printf("%d:%lld\n", index, memory_width_observe(index));
    return 0;
}
