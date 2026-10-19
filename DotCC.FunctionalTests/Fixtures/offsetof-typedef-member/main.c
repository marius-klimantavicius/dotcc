#include <stdio.h>
#include <stddef.h>
typedef long object;
struct Record { char tag; object object; };
struct Rows { int lead; struct Record object[2]; };
int main(void) {
    object value = 7;
    printf("%zu %zu %zu %ld\n", offsetof(struct Record, object),
           offsetof(struct Rows, object[1].object), sizeof(object), value);
    return 0;
}
