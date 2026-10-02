#include <stdlib.h>
#include <inttypes.h>
#include <stdint.h>
#include <limits.h>
#include <string.h>
#include <errno.h>
#ifdef __cplusplus
extern "C" {
#endif
void qsort_r(void *,size_t,size_t,int (*)(const void *,const void *,void *),void *);
static unsigned long digest=1469598103934665603UL;
static int owner_tag, direction=1, comparisons, nested;
static void hash(unsigned long value){digest=(digest^value)*1099511628211UL;}
void algorithms_owner(int tag){owner_tag=tag;}
int algorithms_tag(void){return owner_tag;}
unsigned long algorithms_digest(void){return digest;}
static int integer_compare(const void *a,const void *b){
 int x=*(const int*)a,y=*(const int*)b;++comparisons;
 if(!nested){int small[3]={3,1,2};nested=1;qsort(small,3,sizeof(int),integer_compare);nested=2;if(small[0]!=(direction>0?1:3))abort();}
 return direction*((x>y)-(x<y));
}
struct compare_context {size_t width;int order;int calls;};
static int record_compare(const void *a,const void *b,void *context){
 struct compare_context *c=(struct compare_context*)context;++c->calls;
 const unsigned char *x=(const unsigned char*)a,*y=(const unsigned char*)b;
 size_t n=c->width<3?c->width:3;
 for(size_t i=0;i<n;++i)if(x[i]!=y[i])return c->order*((x[i]>y[i])?1:-1);
 return 0;
}
static int count_compare(const void *a,const void *b){(void)a;(void)b;++comparisons;return 0;}
static unsigned random_value(unsigned *seed){*seed=*seed*1664525U+1013904223U;return *seed;}
static int integer_utilities(void){
 static const long long values[]={LLONG_MIN,LLONG_MIN+1,-9223372036854775807LL,-2147483648LL,-101,-17,-1,0,1,2,17,101,2147483647LL,LLONG_MAX};
 for(size_t i=0;i<sizeof values/sizeof values[0];++i){long long a=values[i];
  if(a!=LLONG_MIN){if(llabs(a)!=(a<0?-a:a)||labs((long)a)!=(a<0?-a:a)||imaxabs((intmax_t)a)!=(a<0?-a:a))return 1;hash((unsigned long)imaxabs(a));}
  for(size_t j=0;j<sizeof values/sizeof values[0];++j){long long b=values[j];if(!b||(a==LLONG_MIN&&b==-1))continue;
   ldiv_t l=ldiv((long)a,(long)b);lldiv_t ll=lldiv(a,b);imaxdiv_t im=imaxdiv(a,b);
   if(l.quot!=a/b||l.rem!=a%b||ll.quot!=l.quot||ll.rem!=l.rem||im.quot!=l.quot||im.rem!=l.rem)return 2;
   hash((unsigned long)im.quot);hash((unsigned long)im.rem);
  }
 }
 for(int a=-101;a<=101;++a){if(abs(a)!=(a<0?-a:a))return 3;for(int b=-101;b<=101;++b){if(!b)continue;div_t d=div(a,b);if(d.quot!=a/b||d.rem!=a%b)return 4;hash((unsigned)d.quot);hash((unsigned)d.rem);}}
 div_t (*df)(int,int)=div;imaxdiv_t (*mf)(intmax_t,intmax_t)=imaxdiv;intmax_t (*af)(intmax_t)=imaxabs;
 if(df(-19,7).rem!=-5||mf(-19,7).quot!=-2||af(-23)!=23)return 5;
 return 0;
}
int algorithms_cases(void){
 digest=1469598103934665603UL;errno=71;int saved_tag=owner_tag;int r=integer_utilities();if(r)return r;
 unsigned seed=3571;int buffer[2048];
 void (*sort)(void*,size_t,size_t,int(*)(const void*,const void*))=qsort;
 void *(*search)(const void*,const void*,size_t,size_t,int(*)(const void*,const void*))=bsearch;
 for(int order=-1;order<=1;order+=2){direction=order;
  for(int pattern=0;pattern<8;++pattern)for(int count=0;count<=2048;count=count<8?count+1:count*2){
   long before=0,after=0;
   for(int i=0;i<count;++i){int v=(int)(random_value(&seed)%2001)-1000;if(pattern==0)v=i;else if(pattern==1)v=count-i;else if(pattern==2)v=7;else if(pattern==3)v=i%5;else if(pattern==4)v=i<count/2?i:count-i;else if(pattern==5)v=(i&1)?1000:-1000;else if(pattern==6)v=(i==count/2)?-1:i;buffer[i]=v;before+=v;}
   comparisons=0;nested=0;sort(buffer,(size_t)count,sizeof(int),integer_compare);
   if(comparisons>count*128+16)return 6;
   for(int i=0;i<count;++i){after+=buffer[i];if(i&&order*((buffer[i-1]>buffer[i])-(buffer[i-1]<buffer[i]))>0)return 7;hash((unsigned)buffer[i]);}
   if(before!=after)return 8;
   for(int key=-1002;key<=2050;key+=23){int present=0;for(int i=0;i<count;++i)if(buffer[i]==key)present=1;int *found=(int*)search(&key,buffer,(size_t)count,sizeof(int),integer_compare);if((found!=0)!=present||(found&&(*found!=key||found<buffer||found>=buffer+count)))return 9;}
  }
 }
 size_t widths[]={1,3,8,257,1025};
 for(size_t w=0;w<5;++w){size_t width=widths[w],count=257;unsigned char *rows=(unsigned char*)malloc(width*count);if(!rows)return 10;
  for(size_t i=0;i<count;++i)for(size_t j=0;j<width;++j)rows[i*width+j]=(unsigned char)((i*97+j*11)%251);
  unsigned long sum=0;for(size_t i=0;i<count*width;++i)sum+=rows[i];
  struct compare_context c={width,(w&1)?-1:1,0};qsort_r(rows,count,width,record_compare,&c);
  if(!c.calls){free(rows);return 11;}for(size_t i=1;i<count;++i)if(record_compare(rows+(i-1)*width,rows+i*width,&c)>0){free(rows);return 12;}
  unsigned long actual=0;for(size_t i=0;i<count*width;++i){actual+=rows[i];hash(rows[i]);}free(rows);if(sum!=actual)return 13;
 }
 comparisons=0;qsort(buffer,0,sizeof(int),count_compare);if(bsearch(buffer,buffer,0,sizeof(int),count_compare)||comparisons)return 14;
 if(errno!=71||owner_tag!=saved_tag)return 15;
 return 0;
}
#ifdef __cplusplus
static int throwing_compare(const void*,const void*){throw 73;}
#endif
int algorithms_exception(void){
#ifdef __cplusplus
 int data[5]={5,4,3,2,1};try{qsort(data,5,sizeof(int),throwing_compare);}catch(int value){return value==73?0:1;}return 2;
#else
 return 0;
#endif
}
#ifdef __cplusplus
}
#endif
