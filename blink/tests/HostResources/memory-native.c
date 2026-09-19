#include "HostMemory.h"
#include <errno.h>
#include <stddef.h>
#include <stdio.h>
int main(void) {
 if(BlinkHostMemoryLimit()!=0)return 1;
 if(BlinkHostMemoryBegin((size_t)-1)!=-1 || errno!=EINVAL)return 2;
 if(BlinkHostMemoryBegin(32768)!=0 || BlinkHostMemoryLimit()!=32768)return 3;
 if(BlinkHostMemoryBegin(65536)!=-1 || errno!=EBUSY || BlinkHostMemoryLimit()!=32768)return 4;
 if(BlinkHostMemoryEnd()!=0 || BlinkHostMemoryLimit()!=0)return 5;
 if(BlinkHostMemoryBegin(65536)!=0 || BlinkHostMemoryLimit()!=65536)return 6;
 BlinkHostMemoryDisposeWorker();if(BlinkHostMemoryLimit()!=0)return 7;
 puts("actual native mapping-owner budget lifecycle: PASS");return 0;
}
