/* Independent narrow-printf pointer profile oracle for the documented musl
   wide-core %jp correction. No wide-core parser or sink adapter is used. */
#include <wchar.h>
#include <stdarg.h>
#include <stdint.h>
#include <string.h>
#include <errno.h>
int __edg_musl_vsnprintf(char *,size_t,const char *,va_list);
void wprintf_case(int,const wchar_t*,unsigned,void*);
static int narrow(char *text,const char *format,...) {
 va_list ap;va_start(ap,format);int result=__edg_musl_vsnprintf(text,256,format,ap);va_end(ap);return result;
}
void wprintf_reference_case(int kind,const wchar_t *format,unsigned capacity,void *record) {
 if(kind!=5){wprintf_case(kind,format,capacity,record);return;}
 wchar_t text[256];int n=-99,result=-1;for(int i=0;i<256;++i)text[i]=0x7b7b;errno=17;
 if(capacity){char fmt[256],bytes[256];unsigned i;for(i=0;format[i];++i)fmt[i]=(char)format[i];fmt[i]=0;
  int count=narrow(bytes,fmt,(void*)(uintptr_t)0x1234,(void*)0,&n);
  unsigned copied=count<0?0:(unsigned)count<capacity?(unsigned)count:capacity-1;
  for(i=0;i<copied;++i)text[i]=(unsigned char)bytes[i];text[copied]=0;result=count<0||(unsigned)count>=capacity?-1:count;
 }
 int *data=(int*)record;data[0]=result;data[1]=errno;data[2]=n;data[3]=0x12345678;memcpy(data+4,text,sizeof text);
}
