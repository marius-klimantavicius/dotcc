#include <stdio.h>
#include "probe.c"
static size_t capacity;
int BlinkHostInitializeBoundResourceLimits(struct System *s){return BlinkHostInitializeResourceLimits(s,capacity);}
void ResourceGc(void) {}
int main(void) {
 ResourceSeedAbi();
 capacity=8;int code=ResourceSeedProbe(32769,capacity);if(code){fprintf(stderr,"line %d\n",code);return 1;}
 capacity=19;code=ResourceSeedProbe(65537,capacity);if(code){fprintf(stderr,"line %d\n",code);return 1;}
 puts("guest resource seed pointer/owner invariants: PASS");return 0;
}
