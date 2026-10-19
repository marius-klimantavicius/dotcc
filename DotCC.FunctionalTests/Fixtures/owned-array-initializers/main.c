#include <stdio.h>
int owned_check(void);
int owned_mutate(void);
int owned_static_step(void);
int owned_thread_step(void);
int owned_expressions(int take_first);
int main(void) {
    int check = owned_check() && owned_mutate() && owned_check() &&
        owned_static_step() == 44 && owned_static_step() == 45 &&
        owned_thread_step() == 41 && owned_thread_step() == 42 &&
        owned_expressions(0) && owned_expressions(1);
    printf("owned arrays %d\n", check);
    return check ? 0 : 1;
}
