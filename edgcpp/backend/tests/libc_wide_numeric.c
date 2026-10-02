#include <stdlib.h>
#include <inttypes.h>
#include <stdint.h>
#include <errno.h>
#include <string.h>
#include <wchar.h>
#include <locale.h>
struct number_result {uint64_t bits;unsigned signexp;int error;long consumed;};
#ifdef __cplusplus
extern "C" {
#endif
float wcstof_l(const wchar_t *,wchar_t **,locale_t);
double wcstod_l(const wchar_t *,wchar_t **,locale_t);
long double wcstold_l(const wchar_t *,wchar_t **,locale_t);
long wcstol_l(const wchar_t *,wchar_t **,int,locale_t);
unsigned long wcstoul_l(const wchar_t *,wchar_t **,int,locale_t);
long long wcstoll_l(const wchar_t *,wchar_t **,int,locale_t);
unsigned long long wcstoull_l(const wchar_t *,wchar_t **,int,locale_t);
static int wide_contract(void) {
 const wchar_t input[]={0x2000,0x3000,'4','2',0};wchar_t *end=(wchar_t*)0;
 errno=71;if(wcstol(input,0,10)!=42 || errno!=71)return 1;
 errno=71;if(wcstod(input,0)!=42 || errno!=71)return 2;
 errno=0;if(wcstol_l(input,&end,10,(locale_t)0)!=0 || errno!=EINVAL || end!=input)return 3;
 errno=0;if(wcstold_l(input,&end,(locale_t)-1)!=0 || errno!=EINVAL || end!=input)return 4;
 errno=0;if(wcstof_l(input,0,(locale_t)0)!=0 || errno!=EINVAL)return 5;
 return 0;
}
void number_case(int kind,const char *bytes,int base,void *result) {
 struct number_result *out=(struct number_result*)result;
 if(kind==255){memset(out,0,sizeof *out);out->bits=wide_contract();return;}
 size_t length=strlen(bytes),prefix=((kind>>8)&3)==1?2:0,suffix=((kind>>8)&3)==2?1:0;
 wchar_t *input=(wchar_t*)malloc((length+prefix+suffix+1)*sizeof(wchar_t));
 int localized=(kind&1024)!=0;kind&=255;
 memset(out,0,sizeof *out);if(!input){out->error=ENOMEM;return;}
 size_t i;if(prefix){input[0]=0x2000;input[1]=0x3000;}
 for(i=0;i<length;++i)input[prefix+i]=(unsigned char)bytes[i];
 if(suffix)input[prefix+length]=0x10000;
 input[prefix+length+suffix]=0;wchar_t *end=input;
 locale_t locale=localized?newlocale(LC_ALL_MASK,"C.UTF-8",(locale_t)0):(locale_t)0;
 errno=71;
 switch(kind) {
 case 0:out->bits=(uint64_t)(localized?wcstol_l(input,&end,base,locale):wcstol(input,&end,base));break;
 case 1:out->bits=(uint64_t)(localized?wcstoul_l(input,&end,base,locale):wcstoul(input,&end,base));break;
 case 2:{float x=(localized?wcstof_l(input,&end,locale):wcstof(input,&end));unsigned bits;memcpy(&bits,&x,4);out->bits=bits;break;}
 case 3:{double x=(localized?wcstod_l(input,&end,locale):wcstod(input,&end));memcpy(&out->bits,&x,8);break;}
 case 4:{long double x=(localized?wcstold_l(input,&end,locale):wcstold(input,&end));unsigned short e;memcpy(&out->bits,&x,8);memcpy(&e,(const char*)&x+8,2);out->signexp=e;break;}
 case 9:out->bits=(uint64_t)wcstoimax(input,&end,base);break;
 case 10:out->bits=(uint64_t)wcstoumax(input,&end,base);break;
 case 11:out->bits=(uint64_t)(localized?wcstoll_l(input,&end,base,locale):wcstoll(input,&end,base));break;
 case 12:out->bits=(uint64_t)(localized?wcstoull_l(input,&end,base,locale):wcstoull(input,&end,base));break;
 }
 out->error=errno;out->consumed=end-input;if(locale)freelocale(locale);free(input);
}

#ifdef __cplusplus
}
#endif
