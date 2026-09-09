#include <stdio.h>
#include <stddef.h>

struct Pair { int key, value; };
struct Buffers {
    char tag;
    int values[4];
    unsigned char bytes[3];
    struct Pair pairs[2];
    int *pointers[2];
};
static struct Buffers global;
static int global_array[3];

int main(void) {
    struct Buffers local;
    struct Buffers *pointer = &local;
    int numbers[3];
    printf("%d %d %d %d %d %d\n",
        (char *)&local.values == (char *)&local.values[0],
        (char *)&local.values - (char *)&local == offsetof(struct Buffers, values),
        (char *)&pointer->values == (char *)&pointer->values[0],
        (char *)&global.values == (char *)&global.values[0],
        (char *)&local.bytes == (char *)&local.bytes[0],
        (char *)&local.values == (char *)local.values);
    printf("%d %d %d %d %d %d\n",
        (char *)&numbers == (char *)numbers,
        (char *)&global_array == (char *)global_array,
        (char *)&local.pairs == (char *)&local.pairs[0],
        (char *)&global.pairs == (char *)&global.pairs[0],
        (char *)&local.pointers == (char *)&local.pointers[0],
        (char *)&global.pointers == (char *)&global.pointers[0]);
    return 0;
}
