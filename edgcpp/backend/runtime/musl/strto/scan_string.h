/* String-only subset of musl's shgetc contract. No FILE or platform ABI. */
#ifndef EDG_SCAN_STRING_H
#define EDG_SCAN_STRING_H
#include <stddef.h>
/* Callback input is used only by scanf. Its single pending byte is restored
   to FILE on return; direct narrow/wide numeric cursors retain unbounded rewind. */
struct edg_scan_input { long count, limit; int stopped, last, pending; int (*read)(void *); void *context; };
struct edg_string_scan {const unsigned char *origin, *pos, *mark; unsigned width;};
#define sh_fromstring(f,s) ((f)->width=1,(f)->origin=(f)->pos=(f)->mark=(const unsigned char *)(s))
#define sh_fromwide(f,s) ((f)->width=sizeof(wchar_t),(f)->origin=(f)->pos=(f)->mark=(const unsigned char *)(s))
#define sh_frominput(f,input) ((f)->width=0,(f)->origin=(const unsigned char *)(input),(f)->pos=(f)->mark=(const unsigned char *)0)
static __inline void __edg_scan_limit(struct edg_string_scan *f,long limit)
{
 if(f->width){f->mark=f->pos;return;}
 struct edg_scan_input *input=(struct edg_scan_input *)f->origin;
 input->count=0;input->limit=limit;input->stopped=0;
}
static __inline long __edg_scan_count(struct edg_string_scan *f)
{
 if(f->width)return (f->pos-f->mark)/f->width;
 return ((struct edg_scan_input *)f->origin)->count;
}
#define shlim(f,limit) __edg_scan_limit(f,limit)
#define shcnt(f) __edg_scan_count(f)
static __inline int __edg_scan_get(struct edg_string_scan *f)
{
 if(!f->width){
  struct edg_scan_input *input=(struct edg_scan_input *)f->origin;
  if(input->stopped || (input->limit && input->count>=input->limit)){input->stopped=1;return -1;}
  int c;if(input->pending){input->pending=0;c=input->last;}else c=input->read(input->context);
  if(c<0){input->stopped=1;return -1;}input->last=c;++input->count;return c;
 }
 if(f->width==1)return *f->pos++;
 wchar_t c=*(const wchar_t *)f->pos;f->pos+=f->width;
 /* Original musl wide scanners substitute '@' for non-ASCII and NUL. */
 return c && c<128 ? (unsigned char)c : '@';
}
#define shgetc(f) __edg_scan_get(f)
static __inline void __edg_scan_unget(struct edg_string_scan *f)
{
 if(f->width){f->pos-=f->width;return;}
 struct edg_scan_input *input=(struct edg_scan_input *)f->origin;
 if(!input->stopped){input->pending=1;--input->count;}
}
#define shunget(f) __edg_scan_unget(f)
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
