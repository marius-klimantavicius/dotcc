#include <stdio.h>
#define TAG 'CIUQ'
#if TAG == 0x43495551 && 'A\101' == 0x4141 && '\1\2' == 0x0102
#define VALID 1
#else
#define VALID 0
#endif
int main(void) {
    printf("%d %d %d %d\n", VALID, TAG, 'AB', 'ABCDE');
    printf("%d %d %d %d\n", 'A\n', '\1\2', '\x41G', '\377ABC');
    return 0;
}
