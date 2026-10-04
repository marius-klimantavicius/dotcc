#include <stdarg.h>
struct Large { unsigned long bytes[4375]; };
static struct Large large;
typedef int (*Unary)(int);
static int plus_seven(int value) { return value + 7; }
static int failures;
static int consume(int count,...) {
  va_list args,copy;va_start(args,count);va_copy(copy,args);
  struct Large value=va_arg(args,struct Large);
  struct Large second=va_arg(args,struct Large);
  int promoted=va_arg(args,int);
  double number=va_arg(args,double);
  Unary callback=va_arg(args,Unary);
  void* pointer=va_arg(args,void*);
  int negative=va_arg(args,int);
  struct Large duplicate=va_arg(copy,struct Large);
  va_end(args);va_end(copy);
  return value.bytes[0]!=17 || value.bytes[4374]!=93 || second.bytes[4374]!=93 ||
    duplicate.bytes[4374]!=93 || promoted!=255 || number!=3.5 || callback(9)!=16 ||
    pointer!=&large || negative!=-17;
}
static int forward(va_list args) {
  int first=va_arg(args,int);double second=va_arg(args,double);return first+(int)second;
}
static void empty(int count,...) { va_list args;va_start(args,count);va_end(args); }
static void values(int count,...) {
  va_list args,copy;va_start(args,count);va_copy(copy,args);
  if(forward(args)!=11 || forward(copy)!=11 || va_arg(args,int)!=-3 || va_arg(copy,int)!=-3)++failures;
  va_end(args);va_end(copy);
}
int main(void) {
  large.bytes[0]=17;large.bytes[4374]=93;
  int (*function)(int,...)=consume;
  void (*indirect)(int,...)=values;indirect(3,(short)7,(float)4.0,-3);
  empty(0);
  return function(4,large,large,(unsigned char)255,(float)3.5,plus_seven,(void*)&large,(signed char)-17) || failures;
}
