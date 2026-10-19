#include <stdio.h>
typedef int (*Callback)(int);
int twice(int x) { return x*2; }
struct API { Callback next; };
Callback restore(void *p) { return p; }
int main(void) { struct API api={twice}; Callback cb=*api.next; Callback *slot=&cb;
 void *bits=(void*)cb; Callback other=bits; other=restore(bits);
 printf("%d %d %d %d %d\n",cb(3),(**slot)(4),other(5),(*(*api.next))(6),other==api.next);return 0; }
