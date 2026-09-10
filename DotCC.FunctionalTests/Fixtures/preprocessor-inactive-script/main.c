#include <stdio.h>
#include "disabled.h"
#define STR(x) #x
#define DROP(x) 42
#define SELECTED 1
#if SELECTED
static int value = 42;
#elif 1
$unselected @annotation `command`
#else
\also_skipped
#endif
int main(void) {
    printf("%d %d %d %s\n", value, from_header, DROP($unused), STR($ @ `));
    return 0;
}
