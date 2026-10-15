/* Native reference process only: not a generated application dependency. */
#include <stdio.h>
#include "callbacks.h"

int main(void) {
    printf("result=%d\n", fixture());
    printf("events=%d\n", native_event_count());
    for (int index = 0; index < native_event_count(); ++index) {
        printf("%d:%d\n", index, native_event_at(index));
    }
    return 0;
}
