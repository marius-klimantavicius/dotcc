#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>
#include <stdint.h>
#include <inttypes.h>
#include <stddef.h>
#include <string.h>
#include <errno.h>
#include <locale.h>
#include <wchar.h>
#ifndef SCAN
#define SCAN swscanf
#endif
#ifdef __cplusplus
extern "C" {
#endif
int wscanf_activate(void){return setlocale(LC_ALL,"C.UTF-8")!=0;}
int original_scan(const wchar_t *,const wchar_t *,...);
int wscanf_address(void){int (*fn)(const wchar_t *,const wchar_t *,...)=SCAN;int value=0;return fn(L"17",L"%d",&value)==1&&value==17?0:1;}
static int scan_va(FILE *file,const wchar_t *format,...){va_list ap;va_start(ap,format);int r=vfwscanf(file,format,ap);va_end(ap);return r;}
static int scan_stdin_va(const wchar_t *format,...){va_list ap;va_start(ap,format);int r=vwscanf(format,ap);va_end(ap);return r;}
int wscanf_file_cases(const char *path){
 if(!setlocale(LC_ALL,"C"))return 30;wchar_t wide=0;char narrow[8]={0};errno=0;
 if(swscanf(L"\u20ac",L"%lc",&wide)!=1||wide!=0x20ac)return 31;
 if(swscanf(L"\u20ac",L"%c",narrow)!=EOF||errno!=EILSEQ)return 32;
 wchar_t escaped[2]={0xdf80,0};errno=0;
 if(swscanf(escaped,L"%c",narrow)!=1||(unsigned char)narrow[0]!=0x80||errno)return 33;
 if(!setlocale(LC_ALL,"C.UTF-8"))return 34;
 char *allocated=0;
 if(swscanf(L"\U0001f642",L"%mc",&allocated)!=1 || !allocated || memcmp(allocated,"\xf0\x9f\x99\x82",4))return 20;free(allocated);allocated=0;
 if(swscanf(L"\U0001f642\U0001f642",L"%2mc",&allocated)!=1 || !allocated || memcmp(allocated,"\xf0\x9f\x99\x82\xf0\x9f\x99\x82",8))return 21;free(allocated);
 FILE *file=fopen(path,"w+");if(!file)return 1;
 if(fputws(L" 12\u20ac34 \nabc",file)<0||fseek(file,0,SEEK_SET))return 2;
 int value=0,n=-1;wchar_t text[8]={0};
 if(fwscanf(file,L"%d%n",&value,&n)!=1||value!=12||n!=3||ftell(file)!=3||fgetwc(file)!=0x20ac)return 3;
 if(scan_va(file,L"%d",&value)!=1||value!=34||ftell(file)!=8)return 4;
 if(fwscanf(file,L" %3ls%n",text,&n)!=1||wcscmp(text,L"abc")||n!=5||feof(file)||fgetwc(file)!=WEOF||!feof(file))return 5;
 if(fseek(file,0,SEEK_SET)||feof(file))return 6;
 if(fwscanf(file,L"q")!=0||ftell(file)!=0)return 7;
 FILE *saved=stdin;stdin=file;int result=wscanf(L"%d",&value);stdin=saved;if(result!=1||value!=12)return 8;
 if(fseek(file,0,SEEK_SET))return 9;saved=stdin;stdin=file;result=scan_stdin_va(L"%d",&value);stdin=saved;if(result!=1||value!=12)return 10;
 if(fclose(file))return 11;file=fopen(path,"w");if(!file)return 12;errno=0;
 if(fwscanf(file,L"%d",&value)!=EOF||!ferror(file)||errno!=EBADF)return 13;
 if(fclose(file)||remove(path))return 14;return 0;
}
/* Deterministic byte record; copies only binary80 value bytes, never padding. */
void wscanf_case(int kind,const wchar_t *text,const wchar_t *format,void *output)
{
 unsigned char *out=(unsigned char*)output;memset(out,0xa5,512);int result=-9,n=-19;errno=71;
 if(kind==0){long long a=127,b=-127;char c[8]={'!'};result=SCAN(text,format,&a,&b,&n,&c);memcpy(out+16,&a,8);memcpy(out+24,&b,8);out[32]=c[0];}
 else if(kind==1){float a=-19;double b=-23;long double c=-29;char tail[8]={'!'};result=SCAN(text,format,&a,&b,&c,&n,&tail);memcpy(out+16,&a,4);memcpy(out+24,&b,8);memcpy(out+32,&c,10);out[42]=tail[0];}
 else if(kind==2){char a[80],b[80],c[8]={'!'};memset(a,'A',80);memset(b,'B',80);result=SCAN(text,format,a,b,&n,&c);memcpy(out+16,a,80);memcpy(out+96,b,80);out[176]=c[0];}
 else if(kind==3){wchar_t a[40],b[40];for(int i=0;i<40;++i){a[i]=0x1234;b[i]=0x5678;}char c[8]={'!'};result=SCAN(text,format,a,b,&n,&c);memcpy(out+16,a,160);memcpy(out+176,b,160);out[336]=c[0];}
 else if(kind==4){char *a=0,*b=0,c[8]={'!'};result=SCAN(text,format,&a,&b,&n,&c);if(a){size_t len=strlen(a);memcpy(out+16,a,len+1);free(a);}if(b){size_t len=strlen(b);memcpy(out+176,b,len+1);free(b);}out[336]=c[0];}
 else if(kind==5){int a=-7,b=-11;result=SCAN(text,format,&a,&b,&n);memcpy(out+16,&a,4);memcpy(out+24,&b,4);}
 else if(kind==6){signed char a=-3;short b=-5;int c=-7;long d=-11;long long e=-13;intmax_t f=-17;size_t g=19;ptrdiff_t h=-23;result=SCAN(text,format,&a,&b,&c,&d,&e,&f,&g,&h,&n);memcpy(out+16,&a,1);memcpy(out+24,&b,2);memcpy(out+32,&c,4);memcpy(out+40,&d,8);memcpy(out+48,&e,8);memcpy(out+56,&f,8);memcpy(out+64,&g,8);memcpy(out+72,&h,8);}
 else if(kind==7){wchar_t *a=0,*b=0;char tail[8]={'!'};result=SCAN(text,format,&a,&b,&n,&tail);if(a){size_t length=wcslen(a);memcpy(out+16,a,(length+1)*4);free(a);}if(b){size_t length=wcslen(b);memcpy(out+192,b,(length+1)*4);free(b);}out[368]=tail[0];}
 int error=errno;memcpy(out,&result,4);memcpy(out+4,&error,4);memcpy(out+8,&n,4);
}
#ifdef __cplusplus
}
#endif
