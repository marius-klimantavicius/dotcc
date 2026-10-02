//type: fp
//options: 
static void bar(void) __attribute__ ((weakref("baz")));
void *x = (void *)bar;
