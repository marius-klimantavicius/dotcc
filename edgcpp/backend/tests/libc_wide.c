#include <wchar.h>
#include <wctype.h>
#include <locale.h>
#include <errno.h>
#include <string.h>
#ifdef __cplusplus
extern "C" {
#endif
unsigned long wide_digest(void)
{
    unsigned long hash=1469598103934665603UL;
    int (*classify[])(wint_t)={iswalnum,iswalpha,iswblank,iswcntrl,iswdigit,iswgraph,iswlower,iswprint,iswpunct,iswspace,iswupper,iswxdigit};
    wint_t c;int i;
    for(c=0;c<0x110000;++c){
        unsigned int bits=0;
        for(i=0;i<12;++i)bits|=(unsigned int)(classify[i](c)!=0)<<i;
        hash=(hash^bits)*1099511628211UL;
        hash=(hash^towlower(c))*1099511628211UL;
        hash=(hash^towupper(c))*1099511628211UL;
    }
    return hash;
}
static int wide_search_cases(void)
{
    wchar_t hay[512],needle[96];unsigned long state=1;size_t round,i,j,hn,nn;
    for(round=0;round<2000;++round){
        state=state*6364136223846793005UL+1;hn=(state>>32)%512;
        state=state*6364136223846793005UL+1;nn=(state>>32)%96;
        for(i=0;i<hn;++i){state=state*6364136223846793005UL+1;hay[i]=(wchar_t)(1+(state>>32)%9);}
        for(i=0;i<nn;++i){state=state*6364136223846793005UL+1;needle[i]=(wchar_t)(1+(state>>32)%9);}
        if(round%7==0){for(i=0;i<hn;++i)hay[i]='a';for(i=0;i<nn;++i)needle[i]='a';if(nn)needle[nn-1]='b';}
        if(round%3==0 && nn<=hn){size_t at=hn-nn;for(i=0;i<nn;++i)hay[at+i]=needle[i];}
        hay[hn]=0;needle[nn]=0;wchar_t *expected=0;
        for(i=0;i<=hn && nn<=hn-i;++i){for(j=0;j<nn && hay[i+j]==needle[j];++j);if(j==nn){expected=hay+i;break;}}
        if(wcsstr(hay,needle)!=expected)return 1;
    }
    return 0;
}
int wide_cases(void)
{
    static const char *names[]={"alnum","alpha","blank","cntrl","digit","graph","lower","print","punct","space","upper","xdigit"};
    int (*classify[])(wint_t)={iswalnum,iswalpha,iswblank,iswcntrl,iswdigit,iswgraph,iswlower,iswprint,iswpunct,iswspace,iswupper,iswxdigit};
    int (*localized[])(wint_t,locale_t)={iswalnum_l,iswalpha_l,iswblank_l,iswcntrl_l,iswdigit_l,iswgraph_l,iswlower_l,iswprint_l,iswpunct_l,iswspace_l,iswupper_l,iswxdigit_l};
    wint_t values[]={0,'A','a','0',' ',9,0x85,0xa0,0x1680,0x180e,0x2000,0x2007,0x2028,0x2029,0x202f,0x205f,0x3000,0xd800,0xdf80,0xe000,0xfffe,0xffff,0x10000,0x20000,0x2fffd,0x2fffe,0x10ffff,0x110000,0xffffffffU};
    wchar_t data[32],copy[32],high[]={0x10000,0},low[]={0xffff,0},*cursor,*found;
    size_t i,j;locale_t locale=newlocale(LC_ALL_MASK,"C.UTF-8",(locale_t)0);
    if(!locale)return 1;errno=71;
    for(i=0;i<sizeof(values)/sizeof(*values);++i){
        for(j=0;j<12;++j){wctype_t descriptor=wctype(names[j]);if(!descriptor || !!iswctype(values[i],descriptor)!=!!classify[j](values[i]) || !!localized[j](values[i],locale)!=!!classify[j](values[i]) || !!iswctype_l(values[i],wctype_l(names[j],locale),locale)!=!!classify[j](values[i]))return 2;}
        if(towctrans(values[i],wctrans("tolower"))!=towlower(values[i]) || towctrans_l(values[i],wctrans_l("toupper",locale),locale)!=towupper(values[i]) || towlower_l(values[i],locale)!=towlower(values[i]) || towupper_l(values[i],locale)!=towupper(values[i]))return 3;
    }
    if(wctype("missing") || wctrans("missing") || errno!=71)return 4;
    if(wcslen(L"a\U0001f600b")!=3 || wcscmp(L"abc",L"abd")>=0 || wcsncmp(L"abc",L"abd",2)!=0)return 5;
    if(wcscpy(data,L"abc")!=data || wcscat(data,L"def")!=data || wcscmp(data,L"abcdef"))return 6;
    if(wcsncpy(copy,data,3)!=copy)return 7;copy[3]=0;if(wcsncat(copy,L"XYZ",2)!=copy || wcscmp(copy,L"abcXY"))return 8;
    wmemset(copy,77,8);wcsncpy(copy,L"x",4);if(copy[0]!='x'||copy[1]||copy[2]||copy[3]||copy[4]!=77)return 9;
    if(wcschr(data,'c')!=data+2 || wcsrchr(data,'a')!=data || wcschr(data,0)!=data+6 || wcsrchr(data,0)!=data+6)return 10;
    if(wcscspn(data,L"dx")!=3 || wcsspn(data,L"abx")!=2 || wcspbrk(data,L"dy")!=data+3)return 11;
    if(wcsstr(data,L"cde")!=data+2 || wcsstr(data,L"")!=data || wcsstr(data,L"cx"))return 12;
    wmemcpy(copy,data,7);if(wmemcmp(copy,data,7) || wmemchr(copy,'e',7)!=copy+4 || wmemchr(copy,'e',4))return 13;
    wmemmove(copy+1,copy,6);if(wmemcmp(copy,L"aabcdef",7))return 14;wmemmove(copy,copy+1,6);copy[6]=0;if(wcscmp(copy,data))return 15;
    wcscpy(copy,L",a,,bc,");found=wcstok(copy,L",",&cursor);if(!found||wcscmp(found,L"a"))return 16;found=wcstok(0,L",",&cursor);if(!found||wcscmp(found,L"bc")||wcstok(0,L",",&cursor))return 17;
    if(wcscoll(high,low)<=0 || wcscoll_l(high,low,locale)<=0)return 18;
    if(wcsxfrm(0,L"abc",0)!=3 || wcsxfrm_l(0,L"abc",0,locale)!=3)return 19;
    for(i=1;i<8;++i){wmemset(copy,77,8);if(wcsxfrm(copy,L"abc",i)!=3 || copy[7]!=77)return 20;if(i>3&&wcscmp(copy,L"abc"))return 21;wmemset(copy,77,8);if(wcsxfrm_l(copy,L"abc",i,locale)!=3 || copy[7]!=77)return 22;if(i>3&&wcscmp(copy,L"abc"))return 23;}
    if(errno!=71)return 24;if(wide_search_cases())return 25;freelocale(locale);return 0;
}
void *wide_locale_create(void){return (void*)newlocale(LC_ALL_MASK,"C.UTF-8",(locale_t)0);}
void wide_locale_free(void *handle){freelocale((locale_t)handle);}
int wide_invalid_locale(void *handle)
{
    locale_t locale=(locale_t)handle;errno=71;
    if(iswalpha_l('A',locale)!=0 || towupper_l('a',locale)!='a' || wctype_l("alpha",locale)!=0 || wctrans_l("tolower",locale)!=0 || towctrans_l('A',(wctrans_t)2,locale)!='A')return 1;
    if(wcsxfrm_l(0,0,0,locale)!=0 || wcscoll_l(0,0,locale)!=0 || errno!=EINVAL)return 2;
    return 0;
}
#ifdef __cplusplus
}
#endif
