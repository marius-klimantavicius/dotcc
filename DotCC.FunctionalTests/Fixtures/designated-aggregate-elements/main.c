#include <stdio.h>
#include <assert.h>
struct metadata { unsigned char valid; signed char format; short flags; };
static const struct metadata table[8] = {
    [5]={1,2,4}, [2]={.flags=6,.valid=1}, {1,3,8}, [5]={1,4,16}
};
int main(void) {
    struct metadata local[] = {[4]={1,2,3},[1]={.flags=7}};
    int scalar[3]={[1]={4}};
    assert(table[0].valid==0 && table[2].flags==6 && table[3].flags==8 && table[5].flags==16);
    assert(local[0].valid==0 && local[1].flags==7 && local[4].flags==3 && scalar[1]==4);
    printf("%d %d %d %d %zu %zu %d\n",table[0].valid,table[2].flags,table[3].flags,
        table[5].flags,sizeof(table),sizeof(local),scalar[1]);
    return 0;
}
