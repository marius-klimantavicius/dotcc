#include <stdarg.h>
struct Pair { long first; double second; };
static int failures;
static int cleanups;
struct Guard { ~Guard(){++cleanups;} };
static long forward(va_list arguments) {
  long result=va_arg(arguments,int);
  result+=(long)va_arg(arguments,double);
  result+=va_arg(arguments,long);
  return result;
}
static int consume(int count,...) {
  va_list first,copy;
  va_start(first,count);
  va_copy(copy,first);
  if(forward(first)!=22 || forward(copy)!=22)++failures;
  long double extended=va_arg(first,long double);
  Pair pair=va_arg(first,Pair);
  __int128 wide=va_arg(first,__int128);
  int* pointer=va_arg(first,int*);
  if(extended!=0x1.000000000000001p0L || pair.first!=71 || pair.second!=3.5 ||
     wide!=(((__int128)1<<100)+91) || *pointer!=42)++failures;
  va_end(first);va_end(copy);return count;
}
static int recursive(int depth,...) {
  va_list args;va_start(args,depth);int value=va_arg(args,int);
  int result=depth?recursive(depth-1,value+1):value;
  va_end(args);return result;
}
static void no_extras(int value,...) {va_list args;va_start(args,value);va_end(args);}
static void raises(int value,...) {Guard guard;va_list args;va_start(args,value);throw va_arg(args,int);}
int main() {
  int pointed=42;Pair pair={71,3.5};
  int (*call)(int,...)=consume;
  int count=call(9,(short)7,(float)6.0,9L,0x1.000000000000001p0L,pair,((__int128)1<<100)+91,&pointed);
  if(count!=9 || recursive(4,8)!=12)++failures;
  no_extras(1);
  try {raises(1,29);} catch(int value) {if(value!=29)++failures;}
  return failures || cleanups!=1;
}
