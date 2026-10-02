//type:fp
//options_all:--g++
//remark:[6.3] GNU compatibility: arguments to the "malloc" attribute
// 9/30/21  [EDGcpfe/24729]
//
// GNU compatibility: arguments to the "malloc" attribute
//
// The front end now supports arguments to the GNU "malloc" attribute
// (used in some later GNU header files).
void foo(void *);
void bar(void *);
void *f(void) __attribute((malloc));
void *g(void) __attribute((malloc(foo)));
void *h(void) __attribute((malloc(bar,1)));
