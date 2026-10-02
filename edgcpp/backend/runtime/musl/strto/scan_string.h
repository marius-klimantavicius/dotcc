/* String-only subset of musl's shgetc contract. No FILE or platform ABI. */
#ifndef EDG_SCAN_STRING_H
#define EDG_SCAN_STRING_H
#include <stddef.h>
struct edg_string_scan {const unsigned char *origin, *pos, *mark; unsigned width;};
#define sh_fromstring(f,s) ((f)->width=1,(f)->origin=(f)->pos=(f)->mark=(const unsigned char *)(s))
#define sh_fromwide(f,s) ((f)->width=sizeof(wchar_t),(f)->origin=(f)->pos=(f)->mark=(const unsigned char *)(s))
#define shlim(f,limit) ((f)->mark=(f)->pos)
#define shcnt(f) (((f)->pos-(f)->mark)/(f)->width)
static __inline int __edg_scan_get(struct edg_string_scan *f)
{
 if(f->width==1)return *f->pos++;
 wchar_t c=*(const wchar_t *)f->pos;f->pos+=f->width;
 /* Original musl wide scanners substitute '@' for non-ASCII and NUL. */
 return c && c<128 ? (unsigned char)c : '@';
}
#define shgetc(f) __edg_scan_get(f)
#define shunget(f) ((void)((f)->pos-=(f)->width))
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
