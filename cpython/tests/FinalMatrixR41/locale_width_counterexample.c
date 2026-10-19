/* Independent diagnostic for CPython 3.13.15 Python/fileutils.c's generic
 * locale algorithm: _Py_mbstowcs checks each wchar as a scalar, while
 * encode_current_locale calls wcstombs on one wchar at a time. Same source
 * runs with native wchar4 and DotCC unsigned UTF-16 wchar2. No ABI redefinition
 * or host wchar function is called on a target wchar16 buffer. */
#include <stdio.h>
#include <stdlib.h>
#include <wchar.h>
#include <locale.h>
#include <errno.h>
#include <string.h>

static int valid_unit(wchar_t ch) {
    return !(ch >= 0xd800 && ch <= 0xdfff) && (unsigned long)ch <= 0x10ffff;
}

static void probe(const char *name, const char *input) {
    wchar_t wide[128];
    char bytes[256];
    size_t i;
    for (i=0;i<128;i++) wide[i]=(wchar_t)0x7777;
    for (i=0;i<256;i++) bytes[i]=0x7e;
    errno=0;
    size_t measured=mbstowcs(NULL,input,0);
    int measure_errno=errno;
    errno=0;
    size_t count=mbstowcs(wide,input,128);
    int decode_errno=errno;
    printf("{\"case\":\"%s\",\"inputBytes\":[",name);
    for(i=0;i<strlen(input);i++) printf("%s%u",i?",":"",(unsigned)(unsigned char)input[i]);
    printf("],\"inputLength\":%llu,\"measure\":%lld,\"measureErrno\":%d,\"decoded\":%lld,\"decodeErrno\":%d",
        (unsigned long long)strlen(input),(long long)measured,measure_errno,(long long)count,decode_errno);
    if(count!=(size_t)-1) {
        int valid=1;
        printf(",\"wideUnits\":[");
        for(i=0;i<count;i++) { printf("%s%u",i?",":"",(unsigned)wide[i]); if(!valid_unit(wide[i]))valid=0; }
        printf("],\"wideNul\":%u,\"genericPerUnitValidation\":%d",(unsigned)wide[count],valid);
        errno=0;
        size_t encoded=wcstombs(bytes,wide,256);
        int encode_errno=errno;
        printf(",\"wholeEncoded\":%lld,\"wholeErrno\":%d,\"roundtrip\":%d,\"outputBytes\":[",
            (long long)encoded,encode_errno,encoded==strlen(input)&&memcmp(bytes,input,encoded)==0);
        if(encoded!=(size_t)-1)for(i=0;i<encoded;i++)printf("%s%u",i?",":"",(unsigned)(unsigned char)bytes[i]);
        printf("],\"perUnitEncoding\":[");
        for(i=0;i<count;i++) {
            wchar_t one[2];one[0]=wide[i];one[1]=0;errno=0;
            size_t result=wcstombs(NULL,one,0);int error=errno;
            printf("%s{\"unit\":%u,\"result\":%lld,\"errno\":%d}",i?",":"",(unsigned)wide[i],(long long)result,error);
        }
        printf("]");
    }
    printf("}\n");
}

int main(void) {
    if(!setlocale(LC_CTYPE,"C.UTF-8"))return 2;
    printf("{\"wcharBytes\":%llu,\"wcharUnsigned\":%d,\"locale\":\"%s\"}\n",
        (unsigned long long)sizeof(wchar_t),(wchar_t)-1>0,setlocale(LC_CTYPE,NULL));
    probe("ascii","ascii");
    probe("BMP","BMP:\xc3\xa9\xe2\x82\xac");
    probe("nonBMP","\xf4\x8f\xbf\xbf");
    probe("combined","UCS:\xc3\xa9\xe2\x82\xac\xf4\x8f\xbf\xbf");
    probe("invalid-surrogate-utf8","\xed\xa0\x80");
    probe("invalid-overlong","\xc0\xaf");
    probe("truncated","\xe2\x82");
    wchar_t high[2]={(wchar_t)0xd800,0};
    wchar_t low[2]={(wchar_t)0xdc80,0};
    errno=0;size_t h=wcstombs(NULL,high,0);int he=errno;
    errno=0;size_t l=wcstombs(NULL,low,0);int le=errno;
    printf("{\"loneHigh\":%lld,\"highErrno\":%d,\"loneLow\":%lld,\"lowErrno\":%d}\n",(long long)h,he,(long long)l,le);
    return 0;
}
