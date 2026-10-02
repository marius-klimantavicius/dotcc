unsigned long long trace;
int live;
void event(int n){trace=trace*131+(unsigned)n;}
struct Guard {int n;Guard(int v):n(v){++live;event(n);}~Guard(){event(n+10);--live;}};
int switch_case(int n,void(*hook)()) {
  int v=0;
  switch(n) {
    case -2: case -1:v=3;break;
    default:v=7;
    case 0:v+=11;break;
    case 1:{Guard g(1);hook();v=23;}break;
    case 2:{Guard g(2);try{throw 9;}catch(int e){v=e;}}break;
    case 3 ... 7:v=31;break;
  }
  return v;
}
int duff(int n){int v=0;int count=(n+3)/4;switch(n%4){case 0:do{++v;case 3:++v;case 2:++v;case 1:++v;}while(--count>0);}return v;}
int jumps(int n){int v=0;if(n)goto inside;while(v<5){if(v==0){inside:v+=2;}else ++v;}return v;}
struct Bits {unsigned a:3; signed b:5;unsigned:0;unsigned c:12;unsigned :3;unsigned d:9;};
struct Packed {unsigned long long a:61;unsigned long long b:12;} __attribute__((packed));
union Overlay {Bits bits;unsigned char raw[sizeof(Bits)];};
Bits stored={5,-7,2055,257};
Bits* once(Bits* p,int* count){++*count;return p;}
int rhs(int* count){*count=*count*10+1;return 11;}
Bits* lhs(Bits* p,int* count){*count=*count*10+2;return p;}
unsigned long long double_bits(double n){union {double d;unsigned long long u;}v;v.d=n;return v.u;}
unsigned float_bits(float n){union {float f;unsigned u;}v;v.f=n;return v.u;}
double compute(double a,double b){return (a+b)*(a-b)/(b+1.0);}
float narrow(double a){return (float)a;}
unsigned long long extended_bits(long double value){unsigned long long r=0;unsigned char* p=(unsigned char*)&value;for(int i=0;i<10;++i)r=r*131+p[i];return r;}
long double extended_calc(long double a,long double b,int op){if(op==0)return a+b;if(op==1)return a-b;if(op==2)return a*b;return a/b;}
unsigned long long run_basic_case(int n,void(*hook)()) {
  trace=0;live=0;
  if(n<12)return (unsigned)switch_case(n-3,hook)*1000000ULL+trace;
  if(n<20)return duff(n-11)*100+jumps(n&1);
  if(n==20){Bits b=stored;return b.a+100*(b.b+20)+10000*b.c+100000000ULL*b.d;}
  if(n==21){Bits b={};int count=0;unsigned v=(once(&b,&count)->a=14);return v+100*b.a+10000*count;}
  if(n==22){Bits b={};b.b=-19;int pre=++b.b;int post=b.b++;return pre+100*(post+30)+10000*(b.b+30);}
  if(n==23){Bits b={};int count=0;unsigned v=(lhs(&b,&count)->a=rhs(&count));return v+100*b.a+10000*count;}
  if(n==24){Packed p={0x123456789abcdefULL,0xdef};Packed q=p;q.b+=99;return q.a^(q.b*131ULL);}
  if(n==25){Overlay o={};o.bits.a=7;o.bits.b=-1;o.bits.c=0xabc;o.bits.d=0x1ef;unsigned long long r=0;for(unsigned i=0;i<sizeof(Bits);++i)r=r*131+o.raw[i];return r;}
  if(n==26){Bits b={};b.a=7;b.a<<=1;b.c=0xfff;b.c>>=2;b.d^=0xaff;return b.a+100*b.c+100000*b.d;}
  if(n==27){unsigned x=0x80000000U;unsigned long n=31;x>>=n;x<<=4;return x;}
  if(n==28)return double_bits(compute(12.25,3.5));
  if(n==29)return float_bits(narrow(0x1.000001p0));
  if(n==30)return float_bits(narrow(0x1.000003p0));
  if(n==31)return double_bits(-0.0);
  if(n==32)return double_bits(0x1p-1074);
  if(n==33)return float_bits(0x1p-149f);
  if(n==34){double x=0.0;hook();x=1.0/x;return double_bits(x);}
  if(n==35){double x=0.0;hook();x=x/x;return (x!=x)+10*(x==x)+100*(x<0)+1000*(x>0);}
  if(n==36){double x=-3.75;return (unsigned)((int)x+10);}
  if(n==37){float f=1.5f;double d=2.25;f+=d;d*=f;return double_bits(d);}
  if(n==38){double d=1.25;double* p=&d;hook();double first=++*p;double second=(*p)++;return double_bits(first+second);}
  if(n==39){unsigned long long n=0xffffffffffffffffULL;return double_bits((double)n);}
  if(n==40){long double a=1.0L,b=0x1p-63L;hook();return extended_bits((a+b)-a);}
  if(n==41)return extended_bits(extended_calc(2.0L,3.0L,3));
  if(n==42)return extended_bits(0x1p-16445L);
  if(n==43){long double a=0x1p-16445L;return extended_bits(a/2.0L);}
  if(n==44){long double a=0x1p-16445L;return extended_bits(a*1.5L);}
  if(n==45){long double a=-0.0L;return extended_bits(a);}
  if(n==46){long double a=0x1.00000000000008p0L;return double_bits((double)a);}
  if(n==47){unsigned long long n=~0ULL;long double a=n;return (unsigned long long)a;}
  if(n==48){long double a=-77.75L;return (unsigned)((int)a+100);}
  if(n==49){long double a=3; a+=0.125; ++a;return extended_bits(a);}
  if(n==50){long double a=0;hook();a=a/a;return (a!=a)+10*(a==a)+100*(a<0)+1000*(a>=0);}
  if(n==51){unsigned long long seed=17,result=0;for(int i=0;i<128;++i){
    union {long double value;unsigned short words[8];}a={},b={};
    seed=seed*6364136223846793005ULL+1;*(unsigned long long*)a.words=seed|0x8000000000000000ULL;a.words[4]=(unsigned short)(16300+(seed%160));
    seed=seed*6364136223846793005ULL+1;*(unsigned long long*)b.words=seed|0x8000000000000000ULL;b.words[4]=(unsigned short)(16300+(seed%160));
    for(int op=0;op<4;++op)result=result*131+extended_bits(extended_calc(a.value,b.value,op));
  }return result;}
  if(n==52){unsigned __int128 a=((unsigned __int128)0xfedcba9876543210ULL<<64)|0x123456789abcdefULL;return (unsigned long long)(a>>70);}
  if(n==53){__int128 a=-((__int128)1<<120)+321;return (unsigned long long)(a>>64);}
  if(n==54){unsigned __int128 a=(unsigned __int128)1<<100,b=123456789;hook();return (unsigned long long)(a/b)^(unsigned long long)(a%b);}
  if(n==55){unsigned __int128 a=(unsigned __int128)1<<96;a+=123;a*=71;a>>=32;return (unsigned long long)a;}
  if(n==56){__int128 a=-((__int128)1<<96),b=12345;return (unsigned long long)(a/b)^(unsigned long long)(a%b);}
  if(n==57){__int128 a=-77;unsigned long long b=~0ULL;a+=b;return (unsigned long long)(a>>2);}
  if(n==58){unsigned __int128 a=(unsigned __int128)1<<100;long double b=a;return extended_bits(b);}
  if(n==59){long double a=0x1.23456789abcp100L;unsigned __int128 b=(unsigned __int128)a;return (unsigned long long)(b>>64)^(unsigned long long)b;}
  if(n==60){volatile int x=13;(void)x;int old=x++;x+=7;return old*100+x;}
  if(n==61){int x=71;int* volatile p=&x;volatile int* q=p;*q+=2;return *p;}
  if(n==62){volatile double d=1.25;d*=2;return double_bits(d);}
  if(n==63){volatile long x=77;try{throw &x;}catch(volatile long* p){return *p;}return 0;}
  return 0;
}

unsigned long long read_basic_trace(){return trace;}
