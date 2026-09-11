#include <stdio.h>
extern int (*volatile callback)(int);
extern int (*const table[])(int);
int main(void)
{
    printf("callback=%d table=%d\n", callback(41), table[1](40));
    callback = table[1];
    printf("updated=%d\n", callback(21));
    return 0;
}
