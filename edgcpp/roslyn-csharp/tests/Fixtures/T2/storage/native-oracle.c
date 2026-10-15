/* Native oracle only. Never link this file into a generated application. */
#include <stdio.h>
#include "storage.h"

int main(void) {
    printf("result=%d\n", fixture());
    for (int selector = 0; selector < STORAGE_OBSERVATION_COUNT; ++selector) {
        printf("%d:%d\n", selector, storage_observe(selector));
    }
    return 0;
}
