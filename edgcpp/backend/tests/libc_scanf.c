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
#define SCAN sscanf
#endif
#ifdef __cplusplus
extern "C" {
#endif
int scanf_activate(void){return setlocale(LC_ALL,"C.UTF-8")!=0;}
int original_scan(const char *,const char *,...);
int scanf_address(void){int (*fn)(const char *,const char *,...)=SCAN;int value=0;return fn("17","%d",&value)==1&&value==17?0:1;}
static int scan_va(FILE *file,const char *format,...){va_list ap;va_start(ap,format);int r=vfscanf(file,format,ap);va_end(ap);return r;}
static int scan_stdin_va(const char *format,...){va_list ap;va_start(ap,format);int r=vscanf(format,ap);va_end(ap);return r;}
int scanf_file_cases(const char *path){
 FILE *file=fopen(path,"w+");if(!file)return 1;
 if(fwrite(" 12x34 \nabc",1,11,file)!=11||fseek(file,0,SEEK_SET))return 2;
 int value=0,n=-1;char text[8]={0};
 if(fscanf(file,"%d%n",&value,&n)!=1||value!=12||n!=3||ftell(file)!=3||fgetc(file)!='x')return 3;
 if(scan_va(file,"%d",&value)!=1||value!=34||ftell(file)!=6)return 4;
 if(fscanf(file," %3s%n",text,&n)!=1||strcmp(text,"abc")||n!=5||feof(file)||fgetc(file)!=EOF||!feof(file))return 5;
 if(fseek(file,0,SEEK_SET)||feof(file))return 6;
 if(fscanf(file,"q")!=0||ftell(file)!=0)return 7;
 FILE *saved=stdin;stdin=file;int r=scanf("%d",&value);stdin=saved;if(r!=1||value!=12)return 8;
 if(fseek(file,0,SEEK_SET))return 9;saved=stdin;stdin=file;r=scan_stdin_va("%d",&value);stdin=saved;if(r!=1||value!=12)return 10;
 if(fclose(file))return 11;file=fopen(path,"w");if(!file)return 12;errno=0;
 if(fscanf(file,"%d",&value)!=EOF||!ferror(file)||errno!=EBADF)return 13;
 if(fclose(file)||remove(path))return 14;return 0;
}
/* Deterministic byte record; copies only binary80 value bytes, never padding. */
void scanf_case(int kind,const char *text,const char *format,void *output)
{
 unsigned char *out=(unsigned char*)output;memset(out,0xa5,512);int result=-9,n=-19;errno=71;
 if(kind==0){long long a=127,b=-127;char c='!';result=SCAN(text,format,&a,&b,&n,&c);memcpy(out+16,&a,8);memcpy(out+24,&b,8);out[32]=c;}
 else if(kind==1){float a=-19;double b=-23;long double c=-29;char tail='!';result=SCAN(text,format,&a,&b,&c,&n,&tail);memcpy(out+16,&a,4);memcpy(out+24,&b,8);memcpy(out+32,&c,10);out[42]=tail;}
 else if(kind==2){char a[80],b[80],c='!';memset(a,'A',80);memset(b,'B',80);result=SCAN(text,format,a,b,&n,&c);memcpy(out+16,a,80);memcpy(out+96,b,80);out[176]=c;}
 else if(kind==3){wchar_t a[40],b[40];for(int i=0;i<40;++i){a[i]=0x1234;b[i]=0x5678;}char c='!';result=SCAN(text,format,a,b,&n,&c);memcpy(out+16,a,160);memcpy(out+176,b,160);out[336]=c;}
 else if(kind==4){char *a=0,*b=0,c='!';result=SCAN(text,format,&a,&b,&n,&c);if(a){size_t len=strlen(a);memcpy(out+16,a,len+1);free(a);}if(b){size_t len=strlen(b);memcpy(out+176,b,len+1);free(b);}out[336]=c;}
 else if(kind==5){int a=-7,b=-11;result=SCAN(text,format,&a,&b,&n);memcpy(out+16,&a,4);memcpy(out+24,&b,4);}
 else if(kind==6){signed char a=-3;short b=-5;int c=-7;long d=-11;long long e=-13;intmax_t f=-17;size_t g=19;ptrdiff_t h=-23;result=SCAN(text,format,&a,&b,&c,&d,&e,&f,&g,&h,&n);memcpy(out+16,&a,1);memcpy(out+24,&b,2);memcpy(out+32,&c,4);memcpy(out+40,&d,8);memcpy(out+48,&e,8);memcpy(out+56,&f,8);memcpy(out+64,&g,8);memcpy(out+72,&h,8);}
 int error=errno;memcpy(out,&result,4);memcpy(out+4,&error,4);memcpy(out+8,&n,4);
}
#ifdef __cplusplus
}
#endif
