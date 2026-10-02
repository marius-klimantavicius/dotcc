/* Exhaustive narrow byte domains with real public function addresses. */
#include <ctype.h>
#include <locale.h>
#include <string.h>
#include <errno.h>
#include <stddef.h>
#ifdef __cplusplus
extern "C" {
#endif
unsigned long ctype_digest(int selection)
{
    static const char *names[]={"C","POSIX","C.UTF-8"};
    locale_t locale=newlocale(LC_ALL_MASK,names[selection],(locale_t)0);
    int (*plain[])(int)={isalnum,isalpha,isblank,iscntrl,isdigit,isgraph,islower,isprint,ispunct,isspace,isupper,isxdigit};
    int (*explicit_locale[])(int,locale_t)={isalnum_l,isalpha_l,isblank_l,iscntrl_l,isdigit_l,isgraph_l,islower_l,isprint_l,ispunct_l,isspace_l,isupper_l,isxdigit_l};
    unsigned long digest=1469598103934665603UL;
    int c,j;
    if(!locale || !setlocale(LC_ALL,names[selection]))return 0;
    errno=71;
    for(c=-1;c<256;++c){
        for(j=0;j<12;++j){
            unsigned int a=plain[j](c)!=0,b=explicit_locale[j](c,locale)!=0;
            if(a!=b)return 0;
            digest=(digest^a)*1099511628211UL;
        }
        if(tolower(c)!=tolower_l(c,locale) || toupper(c)!=toupper_l(c,locale))return 0;
        digest=(digest^(unsigned int)tolower(c))*1099511628211UL;
        digest=(digest^(unsigned int)toupper(c))*1099511628211UL;
        digest=(digest^(unsigned int)(isascii(c)!=0))*1099511628211UL;
    }
    if(errno!=71)return 0;
    freelocale(locale);return digest;
}
int ctype_collation(void)
{
    char high[]={ (char)0xff,0},low[]={ (char)0x80,0};
    char output[8];size_t size;
    locale_t locale=newlocale(LC_ALL_MASK,"C.UTF-8",(locale_t)0);
    int (*compare)(const char*,const char*)=strcoll;
    size_t (*transform)(char*,const char*,size_t)=strxfrm;
    if(!locale)return 1;
    if(compare(high,low)<=0 || compare(low,high)>=0 || compare(high,high)!=0)return 2;
    if(strcoll_l(high,low,locale)<=0 || strcoll_l("abc","abcd",locale)>=0)return 3;
    if(transform(0,"abc",0)!=3 || strxfrm_l(0,"abc",0,locale)!=3)return 4;
    for(size=0;size<8;++size){
        memset(output,'!',8);
        if(transform(output,"abc",size)!=3 || output[7]!='!')return 5;
        if(size>3 && strcmp(output,"abc"))return 6;
        memset(output,'!',8);
        if(strxfrm_l(output,"abc",size,locale)!=3 || output[7]!='!')return 7;
        if(size>3 && strcmp(output,"abc"))return 8;
    }
    output[0]='!';if(transform(output,"",1)!=0 || output[0]!=0)return 9;
    errno=71;if(strcoll_l("a","b",locale)>=0 || strxfrm_l(0,"",0,locale)!=0 || errno!=71)return 10;
    freelocale(locale);return 0;
}
#ifdef __cplusplus
}
#endif
