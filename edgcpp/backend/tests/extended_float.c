#include "extended_float_data.h"
typedef union { __float128 value; unsigned long long words[2]; } Quad;
int failure_case,failure_field;
static int same_quad(Quad value,const unsigned long long* words) {
 if((value.words[1]&0x7fff000000000000ULL)==0x7fff000000000000ULL &&
    ((value.words[1]&0xffffffffffffULL)||value.words[0]))
  return words[0]==0 && words[1]==0x7fff800000000000ULL;
 return value.words[0]==words[0] && value.words[1]==words[1];
}
int main(void) {
 for(unsigned i=0;i<sizeof(float_rows)/sizeof(float_rows[0]);++i) {
  const unsigned long long* row=float_rows[i];Quad a,b,values[4];
  a.words[0]=row[0];a.words[1]=row[1];b.words[0]=row[2];b.words[1]=row[3];
  values[0].value=a.value+b.value;values[1].value=a.value-b.value;
  values[2].value=a.value*b.value;values[3].value=a.value/b.value;
  failure_case=i;
  for(int op=0;op<4;++op){failure_field=op;if(!same_quad(values[op],row+4+2*op))return 1;}
  union {float value;unsigned bits;} f;f.value=(float)a.value;
  union {double value;unsigned long long bits;} d;d.value=(double)a.value;
  union {long double value;unsigned long long words[2];} l;l.value=(long double)a.value;
  unsigned fb=f.bits;unsigned long long db=d.bits,ls=l.words[0],le=l.words[1]&0xffff;
  if((fb&0x7f800000)==0x7f800000 && (fb&0x7fffff))fb=0x7fc00000;
  if((db&0x7ff0000000000000ULL)==0x7ff0000000000000ULL && (db&0xfffffffffffffULL))db=0x7ff8000000000000ULL;
  if((le&0x7fff)==0x7fff && ls!=0x8000000000000000ULL){ls=0xc000000000000000ULL;le=0x7fff;}
  failure_field=4;if(fb!=row[12] || db!=row[13] || ls!=row[14] || le!=row[15])return 2;
 }
 failure_case=-1;failure_field=-1;
 __float128 exact=0x1.0000000000000000000000000001p0Q;
 if(exact-1!=0x1p-112Q)return 3;
 unsigned __int128 large=((unsigned __int128)1<<110)+37;
 if((unsigned __int128)(__float128)large!=large)return 4;
 long double extended=0x1.0000000000000002p0L;
 if((long double)(__float128)extended!=extended)return 5;
 __float128 value=2;value++;value*=3;--value;
 if(value!=8 || !__builtin_isfinite(value) || __builtin_signbit(value))return 6;
#ifdef __cplusplus
 try {throw exact;}
 catch(long double) {return 9;}
 catch(__float128 caught) {if(caught!=exact)return 10;}
#endif
 for(unsigned bits=0;bits<65536;++bits) {
  union{_Float16 value;unsigned short bits;} a,b;a.bits=(unsigned short)bits;
  if(__builtin_isnan(a.value))continue;
  b.value=(_Float16)(__float128)a.value;if(a.bits!=b.bits)return 7;
  b.value=(_Float16)(long double)a.value;if(a.bits!=b.bits)return 8;
 }
 return 0;
}
