#include "shared.h"
struct entry global_first={1,.second=2}, global_second={.first=3,4};
int static_values(void) {
    static struct entry a={5,.second=6}, b={.first=7,8,};
    return a.first+a.second+a.third+b.first+b.second+b.third;
}
