#include "cursor.h"
#include <assert.h>
#include <stdio.h>
static int run(int base, ...) {
    va_list cursor,copy;
    va_start(cursor,base);
    va_copy(copy,cursor);
    int (*read)(va_list *)=next;
    int first=next(&cursor);
    int second=relay(&cursor);
    int copied=optional(&copy);
    assert(optional(0)==11);
    int third=read(&cursor);
    va_end(copy); va_end(cursor);
    assert(first==1 && second==2 && copied==1 && third==3);
    return base+first+second+copied+third;
}
int main(void) { printf("%d %d\n",run(10,1,2,3),run(20,1,2,3)); return 0; }
