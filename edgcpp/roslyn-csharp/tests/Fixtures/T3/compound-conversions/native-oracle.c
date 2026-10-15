#include <stdio.h>
extern int fixture(void);
extern int compound_count(void);
extern long long compound_observe(int);
int main(void) {
    printf("result=%d\n", fixture());
    printf("observations=%d\n", compound_count());
    for (int index = 0; index < compound_count(); ++index)
        printf("%d:%lld\n", index, compound_observe(index));
    return 0;
}
