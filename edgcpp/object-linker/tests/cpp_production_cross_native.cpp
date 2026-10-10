#include "cpp_production_cross.h"
extern "C" int printf(const char *, ...);
extern "C" int puts(const char *);
static int trace[128], count;
extern "C" void record_event(int n) { trace[count++]=n; }
int main() {
    for(int value=0;value!=4;++value) {
        count=0; record_event(400+production_cross_catch(value));
        printf("%d",value); for(int i=0;i<count;++i) printf(" %d",trace[i]); puts("");
    }
}
