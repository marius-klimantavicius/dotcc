#include <stdlib.h>
#include <wchar.h>
#include <uchar.h>
#include <string.h>
#include <locale.h>
#include <errno.h>
#ifdef __cplusplus
extern "C" {
#endif
int multibyte_activate(void){return setlocale(LC_ALL,"C.UTF-8")!=0;}
unsigned long multibyte_digest(void)
{
    unsigned long hash=1469598103934665603UL;unsigned int c;char bytes[8],encoded[8];
    if(!multibyte_activate())return 0;
    for(c=0;c<0x110000;++c){
        mbstate_t state={0};wchar_t decoded=0;char32_t decoded32=0;size_t n,used,split,i;
        if(c>=0xd800 && c<=0xdfff)continue;
        n=wcrtomb(bytes,(wchar_t)c,&state);if(n<1 || n>4 || !mbsinit(&state))return 0;
        used=mbrtowc(&decoded,bytes,n,&state);if(used!=(c?n:0) || (unsigned int)decoded!=c || !mbsinit(&state))return 0;
        for(split=1;split<n;++split){
            state=(mbstate_t){0};decoded=77;
            if(mbrtowc(&decoded,bytes,split,&state)!=(size_t)-2 || mbsinit(&state) || decoded!=77)return 0;
            if(mbrtowc(&decoded,bytes+split,n-split,&state)!=n-split || (unsigned int)decoded!=c || !mbsinit(&state))return 0;
        }
        if(mbrtoc32(&decoded32,bytes,n,&state)!=(c?n:0) || decoded32!=c || c32rtomb(encoded,(char32_t)c,&state)!=n || memcmp(bytes,encoded,n))return 0;
        if((c&255)==0 || c<256){
            char16_t lead=0,tail=0;state=(mbstate_t){0};
            if(mbrtoc16(&lead,bytes,n,&state)!=(c?n:0))return 0;
            if(c>=0x10000){
                if(lead!=0xd800+((c-0x10000)>>10) || mbsinit(&state))return 0;
                if(mbrtoc16(&tail,"",0,&state)!=(size_t)-3 || tail!=0xdc00+((c-0x10000)&1023) || !mbsinit(&state))return 0;
                if(c16rtomb(encoded,lead,&state)!=0 || mbsinit(&state) || c16rtomb(encoded,tail,&state)!=n || !mbsinit(&state))return 0;
            }else if(lead!=c || c16rtomb(encoded,lead,&state)!=n)return 0;
            if(memcmp(bytes,encoded,n))return 0;
        }
        hash=(hash^c)*1099511628211UL;
        for(i=0;i<n;++i)hash=(hash^(unsigned char)bytes[i])*1099511628211UL;
    }
    return hash;
}
int multibyte_cases(void)
{
    const char *text="A\xc2\xa2\xe2\x82\xac\xf0\x9f\x98\x80Z",*source;
    const wchar_t expected[]={65,0xa2,0x20ac,0x1f600,90,0},*wide;
    const unsigned char invalid[][5]={{0x80,0,0,0,0},{0xc0,0x80,0,0,0},{0xed,0xa0,0x80,0,0},{0xf4,0x90,0x80,0x80,0},{0xf5,0x80,0x80,0x80,0},{0xe2,0x28,0xa1,0,0}};
    wchar_t output[16],value;char buffer[32];mbstate_t state={0};size_t n,i;
    if(!multibyte_activate())return 1;
    errno=71;source=text;n=mbsrtowcs(output,&source,16,&state);if(n!=5 || source || !mbsinit(&state) || memcmp(output,expected,sizeof(expected)) || errno!=71)return 2;
    source=text;if(mbsrtowcs(0,&source,0,&state)!=5 || source!=text || !mbsinit(&state))return 3;
    source=text;if(mbsrtowcs(output,&source,2,&state)!=2 || source!=text+3 || output[1]!=0xa2)return 4;
    source=text;if(mbsnrtowcs(output,&source,4,16,&state)!=2 || source!=text+4 || mbsinit(&state))return 5;
    if(mbsnrtowcs(output+2,&source,7,14,&state)!=3 || source!=text+11 || !mbsinit(&state))return 6;
    if(mbsnrtowcs(output+5,&source,1,11,&state)!=0 || source || memcmp(output,expected,sizeof(expected)))return 7;
    source=text;if(mbsnrtowcs(0,&source,12,0,&state)!=5 || source!=text)return 8;
    wide=expected;if(wcsrtombs(0,&wide,0,&state)!=11 || wide!=expected)return 9;
    if(wcsrtombs(buffer,&wide,32,&state)!=11 || wide || memcmp(buffer,text,12))return 10;
    wide=expected;if(wcsnrtombs(buffer,&wide,6,4,&state)!=3 || wide!=expected+2)return 11;
    if(wcsnrtombs(buffer+3,&wide,4,29,&state)!=8 || wide || memcmp(buffer,text,12))return 12;
    if(mbstowcs(output,text,16)!=5 || wcstombs(buffer,expected,32)!=11 || memcmp(buffer,text,12))return 13;
    if(mblen(text+3,3)!=3 || mbtowc(&value,text+3,3)!=3 || value!=0x20ac || mbrlen(text+3,3,&state)!=3)return 14;
    for(i=0;i<sizeof(invalid)/sizeof(*invalid);++i){state=(mbstate_t){0};errno=71;if(mbrtowc(&value,(const char*)invalid[i],4,&state)!=(size_t)-1 || errno!=EILSEQ || !mbsinit(&state))return 15;}
    state=(mbstate_t){0};errno=71;if(mbrtowc(&value,text,0,&state)!=(size_t)-2 || errno!=71 || !mbsinit(&state))return 16;
    if(mbrtowc(&value,text+3,1,&state)!=(size_t)-2 || mbrtowc(0,0,0,&state)!=(size_t)-1 || errno!=EILSEQ || !mbsinit(&state))return 17;
    if(wcrtomb(0,0,&state)!=1 || wctomb(0,0)!=0 || mblen(0,0)!=0 || mbtowc(0,0,0)!=0)return 18;
    if(btowc(-1)!=WEOF || btowc(0x80)!=WEOF || wctob(0x20ac)!=-1)return 19;
    for(i=0;i<128;++i)if(btowc((int)i)!=i || wctob((wint_t)i)!=(int)i)return 20;
    if(!setlocale(LC_ALL,"C"))return 21;
    for(i=128;i<256;++i){buffer[0]=(char)i;state=(mbstate_t){0};if(mbrtowc(&value,buffer,1,&state)!=1 || (unsigned)value!=0xdf00+i || wcrtomb(buffer,value,&state)!=1 || (unsigned char)buffer[0]!=i || btowc((int)i)!=(unsigned)value || wctob((wint_t)value)!=(int)i)return 22;}
    if(!multibyte_activate())return 23;return 0;
}
/* Deliberately omit state pointers to test generated per-API owner/thread TLS. */
unsigned long multibyte_hidden(int api,int finish)
{
    const char *bytes=finish?"\x82\xac":"\xe2";size_t n=finish?2:1;wchar_t wc;char16_t c16;char32_t c32;char output[4];
    if(api==0)return mbrtowc(&wc,bytes,n,0);
    if(api==1)return mbrlen(bytes,n,0);
    if(api==2)return mbrtoc16(&c16,bytes,n,0);
    if(api==3)return mbrtoc32(&c32,bytes,n,0);
    if(api==4)return c16rtomb(output,(char16_t)(finish?0xde00:0xd83d),0);
    return mbsnrtowcs(&wc,&bytes,n,1,0);
}
#ifdef __cplusplus
}
#endif
