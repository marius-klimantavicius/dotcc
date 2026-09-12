#include <stdio.h>
#include <stddef.h>
#include <stdint.h>
typedef struct __attribute__((aligned(16))) Pool { void *next; } Pool;
struct __attribute__((aligned(32))) Wide { int value; };
struct Container { char prefix; Pool pool; struct Wide wide; char tail; };
int main(void) {
    Pool pool = {0};
    Pool pools[2] = {{0}, {0}};
    struct Container container = {0};
    container.wide.value = 42;
    printf("layout %d %d %d %d %d %d %d\n", (int)sizeof(Pool), (int)_Alignof(Pool),
        (int)sizeof(struct Container), (int)_Alignof(struct Container),
        (int)offsetof(struct Container, pool), (int)offsetof(struct Container, wide),
        (int)offsetof(struct Container, tail));
    printf("storage %d %d %d value %d\n", (int)((uintptr_t)&pool & 15),
        (int)((uintptr_t)&pools[1] & 15), (int)((uintptr_t)&container & 31), container.wide.value);
    return 0;
}
