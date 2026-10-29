#include <stdint.h>
#include <stddef.h>
#include <stdio.h>
#include <string.h>

struct Container { char lead; long double value; char tail; };
static long double global_value;

_Static_assert(sizeof(long double) == 16, "long double storage size");
_Static_assert(_Alignof(long double) == 16, "long double alignment");
_Static_assert(sizeof(struct Container) == 48, "container size");
_Static_assert(offsetof(struct Container, value) == 16, "value offset");
_Static_assert(offsetof(struct Container, tail) == 32, "tail offset");

int main(void) {
    unsigned char bytes[16] = {0,0,0,0,0,0,0,128,255,63,161,162,163,164,165,166};
    long double local_value;
    long double array[2];
    struct Container container;
    memcpy(&local_value, bytes, 16);
    memcpy(&global_value, bytes, 16);
    memcpy(&container.value, bytes, 16);
    memcpy(&array[1], bytes, 16);
    long double copy = local_value;
    printf("abi %d %d %d %d %d\n", (int)sizeof(local_value), (int)_Alignof(long double),
        (int)sizeof(container), (int)offsetof(struct Container, value), (int)offsetof(struct Container, tail));
    printf("storage %d %d %d %d stride %d bytes %d %d %d %d %d\n",
        (int)((uintptr_t)&local_value & 15), (int)((uintptr_t)&global_value & 15),
        (int)((uintptr_t)&container & 15), (int)((uintptr_t)array & 15),
        (int)((unsigned char *)&array[1] - (unsigned char *)&array[0]),
        memcmp(&local_value, bytes, 16) == 0, memcmp(&global_value, bytes, 16) == 0,
        memcmp(&container.value, bytes, 16) == 0, memcmp(&array[1], bytes, 16) == 0,
        memcmp(&copy, bytes, 10) == 0);
    return 0;
}
