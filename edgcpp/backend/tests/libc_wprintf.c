#include <wchar.h>
#include <stdio.h>
#include <stdarg.h>
#include <stdint.h>
#include <locale.h>
#include <errno.h>
#include <string.h>
#include <float.h>
#ifndef SWPRINTF
#define SWPRINTF swprintf
#endif
#ifdef __cplusplus
extern "C" {
#endif
int SWPRINTF(wchar_t *,size_t,const wchar_t *,...);
int wprintf_activate(void) {return setlocale(LC_ALL,"C.UTF-8")!=0;}
void wprintf_case(int kind,const wchar_t *format,unsigned capacity,void *record) {
 wchar_t text[256];int n=-99,result=-88;for(int i=0;i<256;++i)text[i]=0x7b7b;
 errno=17;
 switch(kind) {
 case 0:result=SWPRINTF(text,capacity,format,-42,123U,&n);break;
 case 1:result=SWPRINTF(text,capacity,format,0.1L,-0.0L,&n);break;
 case 2:result=SWPRINTF(text,capacity,format,"x\342\202\254y",L"\x3b1\x1f642Z",&n);break;
 case 3:result=SWPRINTF(text,capacity,format,(wint_t)'A',(wint_t)0x1f642,&n);break;
 case 4:result=SWPRINTF(text,capacity,format,-9223372036854775807LL,18446744073709551615ULL,&n);break;
 case 5:result=SWPRINTF(text,capacity,format,(void*)(uintptr_t)0x1234,(void*)0,&n);break;
 case 6:result=SWPRINTF(text,capacity,format,0.1,-0.0,&n);break;
 case 7:result=SWPRINTF(text,capacity,format,8,3,1.25L,&n);break;
 case 8:result=SWPRINTF(text,capacity,format,"\xf0\x80",L"bad",&n);break;
 case 9:result=SWPRINTF(text,capacity,format,L"z\xd800","ok",&n);break;
 case 10:case 11:case 12:{long double value=kind==10?0x1p-16445L:kind==11?LDBL_MAX:0x1p-16382L;result=SWPRINTF(text,capacity,format,value,value,&n);break;}
 }
 int *data=(int*)record;data[0]=result;data[1]=errno;data[2]=n;data[3]=0x12345678;memcpy(data+4,text,sizeof text);
}
static int write_va(FILE *file,const wchar_t *format,...) {
 va_list arguments;va_start(arguments,format);int(*volatile target)(FILE*,const wchar_t*,va_list)=vfwprintf;
 int result=target(file,format,arguments);va_end(arguments);return result;
}
static int buffer_va(wchar_t *buffer,size_t count,const wchar_t *format,...) {
 va_list arguments;va_start(arguments,format);int(*volatile target)(wchar_t*,size_t,const wchar_t*,va_list)=vswprintf;
 int result=target(buffer,count,format,arguments);va_end(arguments);return result;
}
static int stdout_va(const wchar_t *format,...) {
 va_list arguments;va_start(arguments,format);int(*volatile target)(const wchar_t*,va_list)=vwprintf;
 int result=target(format,arguments);va_end(arguments);return result;
}
int wprintf_file_case(const char *path) {
 wchar_t buffer[16];if(buffer_va(buffer,16,L"%ls/%d",L"X",4)!=3||wcscmp(buffer,L"X/4"))return 7;
 FILE *f=fopen(path,"w+");if(!f)return 1;int n=-1;
 if(write_va(f,L"[%ls][%08d][%.2Lf]%n",L"\x3b1\x1f642",-42,1.25L,&n)!=20||n!=20)return 2;
 int(*volatile target)(FILE*,const wchar_t*,...)=fwprintf;
 if(target(f,L"|%d",7)!=2)return 8;
 FILE *saved=stdout;stdout=f;
 int a=wprintf(L"|%lc",(wint_t)'Q'),b=stdout_va(L"|%s","R");stdout=saved;
 if(a!=2||b!=2)return 9;
 if(fwide(f,0)<=0||fflush(f)||fseek(f,0,SEEK_SET))return 3;
 const wchar_t *expected=L"[\x3b1\x1f642][-0000042][1.25]|7|Q|R";
 for(unsigned i=0;expected[i];++i)if(fgetwc(f)!=(wint_t)expected[i])return 4;
 if(fgetwc(f)!=WEOF||!feof(f))return 5;
 return fclose(f)?6:0;
}
#ifdef __cplusplus
}
#endif
