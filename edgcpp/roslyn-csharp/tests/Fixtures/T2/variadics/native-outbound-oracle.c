/* Independent native reference process only. */
#include <stdio.h>
#include "outbound.h"
int main(void) {
    printf("result=%d\n", fixture());
    for (int index = 0; index < 10; ++index)
        printf("%d:%d:%.1f\n", index, native_captured_integer(index), native_captured_double(index));
    return 0;
}
