#include <stdio.h>
#include "shared.h"
typedef int marker;
static int sequence;
static int next(void) { return ++sequence; }
int main(void) {
    struct entry a={next(),.second=12}, b={.first=next(),4};
    int scoped;
    {
        struct entry c={next(),.third=a.first}, marker={.second=next(),b.first};
        scoped=c.first+c.second+c.third+marker.first+marker.second+marker.third;
    }
    marker restored=9;
    printf("%d %d %d %d %d %d\n",global_first.first,global_first.second,global_first.third,
           global_second.first,global_second.second,global_second.third);
    printf("%d %d %d %d %d %d %d %d %d %d\n",a.first,a.second,a.third,b.first,b.second,b.third,
           sequence,scoped,restored,static_values());
    return 0;
}
