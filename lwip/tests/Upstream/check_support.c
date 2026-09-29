#include <stdarg.h>
#include <stdio.h>
#include "check.h"
extern void dotcc_check_failure(const char *expression,const char *file,int line,const char *message);
void dotcc_check_fail(const char *expression,const char *file,int line,const char *format,...)
{
  char message[1024]; va_list args; va_start(args,format);
  vsnprintf(message,sizeof(message),format,args); va_end(args);
  dotcc_check_failure(expression,file,line,message);
}
