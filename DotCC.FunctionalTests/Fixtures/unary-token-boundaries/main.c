#include <stdio.h>
static int a = -(-5), b = +(+4), c = -(+3), d = +(-2), e = ~ (~1);
int main(void) { int x=7,y=2; int p=-(-x),q=+(+y),r=- --x,s=+ ++y;
 printf("%d %d %d %d %d %d %d %d %d %d %d\n",a,b,c,d,e,p,q,r,s,x,y); return 0; }
