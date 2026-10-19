#include "../contract.h"
#include <stdio.h>
size_t choose(const size_t value) { return value+1000; }
int main(void) {
    int result=run_c();
    if(result) { printf("formal parameter failure %d\n",result); return result; }
    puts("formal parameters: ordinary/tight/empty/wide, callbacks, promotions, evaluations=2 trace=12");
    return 0;
}
