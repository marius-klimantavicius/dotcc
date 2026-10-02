/* String-only subset of musl's shgetc contract. No FILE or platform ABI. */
#ifndef EDG_SCAN_STRING_H
#define EDG_SCAN_STRING_H
#include <stddef.h>
struct edg_string_scan {const unsigned char *origin, *pos, *mark;};
#define sh_fromstring(f,s) ((f)->origin=(f)->pos=(f)->mark=(const unsigned char *)(s))
#define shlim(f,limit) ((f)->mark=(f)->pos)
#define shcnt(f) ((f)->pos-(f)->mark)
#define shgetc(f) (*(f)->pos++)
#define shunget(f) ((void)(f)->pos--)
#ifdef __cplusplus
extern "C" {
#endif
double __edg_scalbn_nearest(double,int);
long double __edg_scan_scalbnl_nearest(long double,int);
/* Selected core passes finite x and positive finite y; exact remainder. */
long double __edg_scan_remainder(long double,long double);
#ifdef __cplusplus
}
#endif
#endif
