/* Independent oracle-only main; never exported or linked into generated code. */
#include <stdio.h>
int fixture(void);
int program_observation_count(void);
int program_observe(int selector);
int main(void) {
    printf("result=%d\n", fixture());
    int count = program_observation_count();
    printf("observations=%d\n", count);
    for (int selector = 0; selector < count; ++selector) printf("%d:%d\n", selector, program_observe(selector));
    return 0;
}
