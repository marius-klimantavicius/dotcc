/* All three generic operations use mathematical integer inputs before the
   destination conversion. Compare boundary and random bit patterns with GCC,
   Clang and the original EDG C backend; do not apply usual mixed conversions. */
typedef unsigned long long U64;
typedef long long S64;
typedef unsigned __int128 U128;
typedef __int128 S128;
#ifdef __cplusplus
typedef bool Boolean;
#else
typedef _Bool Boolean;
#endif
#ifdef __cplusplus
extern "C" int putchar(int);
#else
extern int putchar(int);
#endif
static U64 digest;
static unsigned failures;
static void mix(U128 bits,int overflow){digest=(digest^(U64)bits)*1099511628211ULL;digest=(digest^(U64)(bits>>64))*1099511628211ULL;digest=(digest^(unsigned)overflow)*1099511628211ULL;}
static void print_hex(U64 value){int shift;for(shift=60;shift>=0;shift-=4)putchar("0123456789abcdef"[(value>>shift)&15]);putchar('\n');}
static U128 patterns[40];
static void seed(void){U64 state=0x517cc1b727220a95ULL;unsigned i;
  U128 fixed[]={0,1,2,127,128,255,256,32767,32768,65535,65536,2147483647U,2147483648U,4294967295U,
    (U128)1<<32,((U128)1<<63)-1,(U128)1<<63,((U128)1<<64)-1,(U128)1<<64,((U128)1<<127)-1,(U128)1<<127,~(U128)0};
  for(i=0;i<22;++i)patterns[i]=fixed[i];
  for(;i<40;++i){state^=state<<13;state^=state>>7;state^=state<<17;patterns[i]=(U128)state<<64;state^=state<<13;state^=state>>7;state^=state<<17;patterns[i]|=state;}
}
#define APPLY(FN,A,B) do{slot.result=(Target)19;int overflow=FN((A),(B),&slot.result);mix((U128)slot.result,overflow);if(slot.before!=0xabcddcba11223344ULL||slot.after!=0x55667788abcdef01ULL)++failures;}while(0)
#define MATRIX(AType,BType,TType) do {typedef TType Target;struct {U64 before;Target result;U64 after;}slot={0xabcddcba11223344ULL,0,0x55667788abcdef01ULL};unsigned i,j;digest=1469598103934665603ULL;for(i=0;i<40;++i)for(j=0;j<40;++j){AType a=(AType)patterns[i];BType b=(BType)patterns[j];APPLY(__builtin_add_overflow,a,b);APPLY(__builtin_sub_overflow,a,b);APPLY(__builtin_mul_overflow,a,b);}print_hex(digest);}while(0)
static int a_calls,b_calls,p_calls;
static S64 alias_value;
static S64 left(void){++a_calls;return alias_value;}
static S64 right(void){++b_calls;return 7;}
static S64* destination(void){++p_calls;return &alias_value;}
#define FIXED(Add,Sub,Mul,T) do{T result;unsigned i,j;digest=1469598103934665603ULL;for(i=0;i<40;++i)for(j=0;j<40;++j){T a=(T)patterns[i],b=(T)patterns[j];int o=Add(a,b,&result);mix((U128)result,o);o=Sub(a,b,&result);mix((U128)result,o);o=Mul(a,b,&result);mix((U128)result,o);}print_hex(digest);}while(0)
int main(void){seed();
  MATRIX(signed char,signed char,signed char);MATRIX(unsigned char,unsigned char,unsigned char);
  MATRIX(short,unsigned short,short);MATRIX(unsigned short,short,unsigned short);
  MATRIX(int,int,int);MATRIX(unsigned,unsigned,unsigned);MATRIX(int,unsigned,int);MATRIX(unsigned,int,unsigned);
  MATRIX(S64,S64,S64);MATRIX(U64,U64,U64);MATRIX(S64,U64,S64);MATRIX(U64,S64,U64);
  MATRIX(U64,U64,S64);MATRIX(S64,S64,U64);MATRIX(U64,S64,signed char);MATRIX(S64,U64,unsigned short);
  MATRIX(S64,U64,S128);MATRIX(S64,U64,U128);MATRIX(U64,U64,S128);MATRIX(U64,U64,U128);
  MATRIX(S128,S128,S128);MATRIX(U128,U128,U128);MATRIX(S128,U128,S128);MATRIX(U128,S128,U128);
  MATRIX(U128,U128,S128);MATRIX(S128,S128,U128);MATRIX(S128,U64,S64);MATRIX(U128,S64,U64);
  MATRIX(U128,S128,signed char);MATRIX(S128,U128,unsigned short);MATRIX(Boolean,S64,S64);MATRIX(S64,Boolean,unsigned);
  FIXED(__builtin_sadd_overflow,__builtin_ssub_overflow,__builtin_smul_overflow,int);
  FIXED(__builtin_uadd_overflow,__builtin_usub_overflow,__builtin_umul_overflow,unsigned);
  FIXED(__builtin_saddl_overflow,__builtin_ssubl_overflow,__builtin_smull_overflow,long);
  FIXED(__builtin_uaddl_overflow,__builtin_usubl_overflow,__builtin_umull_overflow,unsigned long);
  FIXED(__builtin_saddll_overflow,__builtin_ssubll_overflow,__builtin_smulll_overflow,S64);
  FIXED(__builtin_uaddll_overflow,__builtin_usubll_overflow,__builtin_umulll_overflow,U64);
  {int o;volatile int value;digest=0;o=__builtin_add_overflow(2147483647,1,&value);mix((U128)value,o);o=__builtin_mul_overflow(value,2,&value);mix((U128)value,o);print_hex(digest);}
  alias_value=9223372036854775807LL;digest=0;
  {int o=__builtin_add_overflow(left(),right(),destination());mix((U128)alias_value,o);}
  {int o=__builtin_sub_overflow(left(),right(),destination());mix((U128)alias_value,o);}
  {int o=__builtin_mul_overflow(left(),right(),destination());mix((U128)alias_value,o);}
  if(a_calls!=3||b_calls!=3||p_calls!=3)++failures;mix((U128)(a_calls*100+b_calls*10+p_calls),failures);print_hex(digest);
  return failures?1:0;
}
