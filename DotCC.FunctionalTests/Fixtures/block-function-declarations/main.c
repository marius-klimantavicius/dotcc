#include <stdio.h>
static long local(long value) { return value+11; }
int main(void) {
    long local(long);
    long remote(long);
    long first=remote(7);
    int value=9;
    {
        extern long remote(long);
        long (*pointer)(long)=remote;
        long (local)(long);
        value+=local(2);
        printf("%ld %d %d\n",first,value,pointer==remote);
    }
    return 0;
}
