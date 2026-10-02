#include <stdarg.h>
struct Large { unsigned long bytes[4375]; };
static struct Large large;
static int consume(int count,...) {
  va_list args,copy;va_start(args,count);va_copy(copy,args);
  struct Large value=va_arg(args,struct Large);
  struct Large second=va_arg(args,struct Large);
  int promoted=va_arg(args,int);
  double number=va_arg(args,double);
  struct Large duplicate=va_arg(copy,struct Large);
  va_end(args);va_end(copy);
  return value.bytes[0]!=17 || value.bytes[4374]!=93 || second.bytes[4374]!=93 ||
    duplicate.bytes[4374]!=93 || promoted!=255 || number!=3.5;
}
int main(void) {
  large.bytes[0]=17;large.bytes[4374]=93;
  int (*function)(int,...)=consume;
  return function(4,large,large,(unsigned char)255,(float)3.5);
}
