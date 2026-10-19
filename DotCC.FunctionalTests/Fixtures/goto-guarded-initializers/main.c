#include <stdio.h>
static int effects;
int run(int fail) { int ready=0; if(fail) goto done; int value=++effects; ready=1;
 done: if(ready) return value; return -1; }
int loop(void) { int n=0;
 again: ; int value=++effects; if(++n<3) goto again; return value; }
int main(void) { int a=run(1),b=run(0),c=run(1),d=loop();printf("%d %d %d %d %d\n",a,b,c,d,effects);return 0; }
