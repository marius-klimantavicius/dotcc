#include <stdio.h>
struct Inner { int fd; int flag; long extra[2]; };
struct Outer { volatile struct Inner wakeup; };
static struct Outer state={.wakeup={.fd=4,.flag=5,.extra={6,7}}};
static int calls;
struct Outer *get(void) { ++calls; return &state; }
int main(void) { volatile struct Inner local={1,2,{3,4}}; int first=local.fd;
 get()->wakeup.fd=9; int second=get()->wakeup.flag;
 volatile struct Inner *p=&state.wakeup; p->extra[1]=12;
 printf("%d %d %d %ld %d\n",first,second,state.wakeup.fd,p->extra[1],calls);return 0; }
