#include <stdio.h>
typedef int zset;
typedef struct source {
    union {
        struct { int count; } zset;
        int alternative;
    } value;
    zset marker;
} source;
int main(void) { source item = {0}; item.value.zset.count=42; item.marker=7; printf("%d %d\n",item.value.zset.count,item.marker); return 0; }
