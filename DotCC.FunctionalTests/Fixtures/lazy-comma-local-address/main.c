#include <stdio.h>
static int effects;
void touch(void) { ++effects; }
int *choose(int flag,int *p) { return flag?(touch(),p):p; }
int main(void) { int item=7; int *ptr=&item; int enabled=0;
 int *a=enabled?(touch(),ptr):&item; enabled=1;
 int *b=enabled?(touch(),ptr):&item;
 int c=0&&(touch(),item++); int d=1||(touch(),item++);
 int e=enabled&&(touch(),item==7); int f=0||(touch(),item==7);
 printf("%d %d %d %d %d %d %d %d\n",*a,*b,c,d,e,f,effects,*choose(0,&item));return 0; }
