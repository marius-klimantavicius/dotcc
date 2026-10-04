#include <stdarg.h>
struct Pair { long first; double second; };
struct Large { unsigned long words[9000]; };
struct Sequence { int first, second, third; };
typedef int (*Unary)(int);
enum Small : unsigned short { max_small = 65535 };
static int failures;
static int cleanups;
static int evaluations;
static int snapshot_event;
static int picked_event;
static int side_effects[5];
static Large shared;
struct Guard { ~Guard(){++cleanups;} };
static int plus_three(int value) { return value + 3; }
static long forward(va_list arguments) {
  long result=va_arg(arguments,int);
  result+=(long)va_arg(arguments,double);
  result+=va_arg(arguments,long);
  if(va_arg(arguments,int)!=0x31415926)++failures;
  if(va_arg(arguments,unsigned long long)!=11ULL)++failures;
  if(va_arg(arguments,int)!=0x27182818)++failures;
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
  Unary callback=va_arg(first,Unary);
  void* null_pointer=va_arg(first,void*);
  int negative=va_arg(first,int),maximum=va_arg(first,int),boolean=va_arg(first,int);
  int enumeration=va_arg(first,int);
  if(extended!=0x1.000000000000001p0L || pair.first!=71 || pair.second!=3.5 ||
     wide!=(((__int128)1<<100)+91) || *pointer!=42 || callback(11)!=14 || null_pointer!=nullptr ||
     negative!=-12 || maximum!=65535 || boolean!=1 || enumeration!=65535)++failures;
  va_end(first);va_end(copy);return count;
}
static int recursive(int depth,...) {
  va_list args;va_start(args,depth);int value=va_arg(args,int);
  int result=depth?recursive(depth-1,value+1):value;
  va_end(args);return result;
}
static void no_extras(int value,...) {va_list args;va_start(args,value);va_end(args);}
static void raises(int value,...) {Guard guard;va_list args;va_start(args,value);throw va_arg(args,int);}
static void void_consume(int count,...) {
  va_list args;va_start(args,count);
  if(count!=2 || va_arg(args,int)!=-7 || va_arg(args,double)!=2.5)++failures;
  va_end(args);
}
static int mutate_shared() { int event=++evaluations;shared.words[0]=99;shared.words[8999]=101;return event; }
static int snapshot_consume(int count,...) {
  va_list args;va_start(args,count);
  Large value=va_arg(args,Large);
  int mutation_event=va_arg(args,int);
  if(evaluations!=2 || (snapshot_event!=1 && snapshot_event!=2) || mutation_event==snapshot_event)++failures;
  unsigned long first=snapshot_event<mutation_event?17:99;
  unsigned long last=snapshot_event<mutation_event?93:101;
  if(value.words[0]!=first || value.words[8999]!=last)++failures;
  va_end(args);return 19;
}
static int mark(int tag) { ++side_effects[tag];return ++evaluations; }
static int order_consume(int count,...) {
  va_list args;va_start(args,count);
  Sequence value=va_arg(args,Sequence);int last=va_arg(args,int);
  if(picked_event!=1 || evaluations!=5 || value.second!=value.first+1 || value.third!=value.second+1 ||
     !((value.first==2 && last==5) || (last==2 && value.first==3)))++failures;
  for(int i=1;i!=5;++i)if(side_effects[i]!=1)++failures;
  va_end(args);return 23;
}
typedef int (*Variadic)(int,...);
static Variadic pick_consumer() { picked_event=++evaluations;return order_consume; }
static void large_raises(int count,...) {
  Guard guard;va_list args,copy;va_start(args,count);va_copy(copy,args);
  Large first=va_arg(args,Large);Large second=va_arg(copy,Large);int code=va_arg(args,int);
  if(first.words[0]!=17 || first.words[8999]!=93 || second.words[8999]!=93)++failures;
  va_end(args);va_end(copy);throw code;
}
int main() {
  int pointed=42;Pair pair={71,3.5};
  int (*call)(int,...)=consume;
  int count=call(9,(short)7,(float)6.0,9L,0x31415926,11ULL,0x27182818,0x1.000000000000001p0L,pair,((__int128)1<<100)+91,&pointed,
                 plus_three,(void*)nullptr,(signed char)-12,(unsigned short)65535,true,max_small);
  if(count!=9 || recursive(4,8)!=12)++failures;
  no_extras(1);
  void (*void_call)(int,...)=void_consume;void_call(2,-7,(float)2.5);
  void (*empty_call)(int,...)=no_extras;empty_call(3);
  shared.words[0]=17;shared.words[8999]=93;evaluations=0;
  if(snapshot_consume(2,(snapshot_event=++evaluations,shared),mutate_shared())!=19)++failures;
  evaluations=0;
  Variadic selected=pick_consumer();
  if(selected(2,Sequence{mark(1),mark(2),mark(3)},mark(4))!=23)++failures;
  try {raises(1,29);} catch(int value) {if(value!=29)++failures;}
  shared.words[0]=17;shared.words[8999]=93;
  for(int i=0;i!=3;++i)try {large_raises(2,shared,37);} catch(int value) {if(value!=37)++failures;}
  return failures || cleanups!=4;
}
