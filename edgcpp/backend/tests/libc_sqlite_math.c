#include <math.h>
#include <float.h>
#include <stdint.h>
#include <errno.h>
#include <stdio.h>
static double (*unary[])(double)={exp,sin,cos,tan,asin,acos,atan,sinh,cosh,tanh,asinh,acosh,atanh,expm1,log1p};
static double (*binary[])(double,double)={pow,atan2};
static uint64_t hash,state;
static uint64_t bits(void){state=state*6364136223846793005ULL+1442695040888963407ULL;return state;}
static void mix(double result){union{double f;uint64_t i;}v={result};uint64_t value=isnan(result)?0x7ff8000000000000ULL:v.i;hash^=value;hash*=1099511628211ULL;
#ifdef MATH_TRACE
 printf("%llu\n",(unsigned long long)value);
#endif
}
#ifdef __cplusplus
extern "C" {
#endif
void sqlite_math_set_errno(int value){errno=value;}
int sqlite_math_errno(void){return errno;}
double sqlite_math_one(int function,double x,double y){return function<15?unary[function](x):binary[function-15](x,y);}
int sqlite_math_errors(void)
{
#ifdef ADAPTED
 #define CASE(expr,test,expected,n) do{errno=713;double r=(expr);if(!(test)||errno!=(expected))return n;}while(0)
 CASE(exp(1000),isinf(r)&&r>0,ERANGE,1);
 CASE(exp(-1000),r==0&&!signbit(r),ERANGE,2);
 CASE(exp(((double)INFINITY)),isinf(r),713,3);
 CASE(exp(-((double)INFINITY)),r==0,713,4);
 CASE(exp(((double)NAN)),isnan(r),713,5);
 CASE(pow(-2,0.5),isnan(r),EDOM,6);
 CASE(pow(0,-2),isinf(r)&&r>0,ERANGE,7);
 CASE(pow(-0.0,-3),isinf(r)&&signbit(r),ERANGE,8);
 CASE(pow(2,1024),isinf(r),ERANGE,9);
 CASE(pow(2,-1075),r==0,ERANGE,10);
 CASE(pow(2,-1074),r==DBL_TRUE_MIN,713,11);
 CASE(pow(0,0),r==1,713,12);
 CASE(pow(1,((double)NAN)),r==1,713,13);
 CASE(pow(((double)NAN),0),r==1,713,14);
 CASE(pow(-((double)INFINITY),0.5),isinf(r)&&r>0,713,15);
 CASE(sin(((double)INFINITY)),isnan(r),EDOM,16);
 CASE(cos(-((double)INFINITY)),isnan(r),EDOM,17);
 CASE(tan(((double)INFINITY)),isnan(r),EDOM,18);
 CASE(asin(2),isnan(r),EDOM,19);
 CASE(acos(-2),isnan(r),EDOM,20);
 CASE(acosh(0.5),isnan(r),EDOM,21);
 CASE(atanh(2),isnan(r),EDOM,22);
 CASE(atanh(-1),isinf(r)&&signbit(r),ERANGE,23);
 CASE(atanh(1),isinf(r)&&r>0,ERANGE,24);
 CASE(sinh(1000),isinf(r)&&r>0,ERANGE,25);
 CASE(sinh(-1000),isinf(r)&&signbit(r),ERANGE,26);
 CASE(cosh(1000),isinf(r)&&r>0,ERANGE,27);
 CASE(sinh(((double)INFINITY)),isinf(r),713,28);
 CASE(log1p(-1),isinf(r)&&signbit(r),ERANGE,29);
 CASE(log1p(-2),isnan(r),EDOM,30);
 CASE(expm1(1000),isinf(r),ERANGE,31);
 CASE(expm1(-1000),r==-1,713,32);
 CASE(atan2(DBL_TRUE_MIN,DBL_MAX),r==0,ERANGE,33);
 CASE(atan2(0,1),r==0&&!signbit(r),713,34);
 CASE(atan2(-0.0,1),r==0&&signbit(r),713,35);
 CASE(acos(1),r==0&&!signbit(r),713,36);
 CASE(acosh(1),r==0&&!signbit(r),713,37);
 CASE(tanh(((double)INFINITY)),r==1,713,38);
 CASE(asinh(-((double)INFINITY)),isinf(r)&&signbit(r),713,39);
 for(int i=0;i<15;++i){errno=713;if(!isnan(unary[i](((double)NAN)))||errno!=713)return 40+i;}
 #undef CASE
#endif
 return 0;
}
#ifdef __cplusplus
}
#endif
int main(void)
{
 hash=1469598103934665603ULL;state=0x712ea892bad41321ULL;
 double values[]={0,-0.0,DBL_TRUE_MIN,-DBL_TRUE_MIN,DBL_MIN,-DBL_MIN,DBL_MAX,-DBL_MAX,((double)INFINITY),-((double)INFINITY),((double)NAN),
  1,-1,0.5,-0.5,2,-2,10,-10,709,710,-745,-746,0x1.fffffffffffffp-1,0x1.0000000000001p0,-0x1.fffffffffffffp-1,-0x1.0000000000001p0,
  0x1.921fb54442d17p0,0x1.921fb54442d18p0,0x1.921fb54442d19p0,0x1.921fb54442d18p1,0x1.921fb54442d18p2,
  0x1p-30,-0x1p-30,0x1p26,-0x1p26,0x1p100,0x1p500,0x1p1023};
 for(unsigned n=0;n<sizeof(values)/sizeof(values[0]);++n)for(int f=0;f<15;++f)mix(unary[f](values[n]));
 for(unsigned n=0;n<sizeof(values)/sizeof(values[0]);++n)for(unsigned j=0;j<sizeof(values)/sizeof(values[0]);++j)for(int f=0;f<2;++f)mix(binary[f](values[n],values[j]));
 for(int n=0;n<1800;++n){union{double f;uint64_t i;}x,y;x.i=bits();y.i=bits();for(int f=0;f<15;++f)mix(unary[f](x.f));mix(pow(x.f,y.f));mix(atan2(x.f,y.f));}
 for(int n=-1100;n<=1100;++n){double x=(double)n/64;for(int f=0;f<15;++f)mix(unary[f](x));mix(pow(1.0001,(double)n));mix(pow(2,(double)n));}
 int error=sqlite_math_errors();if(error)return error;
 printf("%llu\n",(unsigned long long)hash);return 0;
}
