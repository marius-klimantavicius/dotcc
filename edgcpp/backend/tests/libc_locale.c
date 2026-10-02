#include <locale.h>
#include <stdlib.h>
#include <string.h>
#include <wchar.h>
#include <errno.h>
#include <stdint.h>
#define CHECK(x,n) do {if(!(x))return n;}while(0)
#ifdef __cplusplus
extern "C" {
#endif
int locale_cases(void)
{
    CHECK(uselocale(0)==LC_GLOBAL_LOCALE,1);
    locale_t c=newlocale(LC_ALL_MASK,"C",0),u=newlocale(LC_ALL_MASK,"C.UTF-8",0);
    CHECK(c && u && c!=u,2);CHECK(uselocale(u)==LC_GLOBAL_LOCALE && MB_CUR_MAX==4,3);
    CHECK(setlocale(LC_ALL,"C")!=0 && MB_CUR_MAX==4,4);
    char buf[4];CHECK(wctomb(buf,0x20ac)==3 && (unsigned char)buf[0]==0xe2,5);
    locale_t d=duplocale(u);CHECK(d && d!=u,6);CHECK(uselocale(c)==u && MB_CUR_MAX==1,7);
    char *end;errno=71;CHECK(strtod_l("  -12.5tail",&end,u)==-12.5 && *end=='t' && errno==71 && MB_CUR_MAX==1,8);
    CHECK(strtof_l("0x1p-149x",&end,u)>0 && *end=='x' && errno==ERANGE,9);
    CHECK(strtold_l("1.0000000000000000001084202172485504434x",&end,u)>1.0L && *end=='x',10);
    CHECK(strtol_l("-ff!",&end,16,u)==-255 && *end=='!',11);
    CHECK(strtoull_l("18446744073709551616",&end,10,u)==UINT64_MAX && !*end && errno==ERANGE,12);
    CHECK(uselocale(LC_GLOBAL_LOCALE)==c,13);freelocale(c);freelocale(u);freelocale(d);
    struct lconv *info=localeconv();CHECK(info && !strcmp(info->decimal_point,".") && !*info->thousands_sep && info->int_frac_digits==127 && info->int_n_sign_posn==127,14);
    CHECK(newlocale(LC_ALL_MASK,"unsupported_locale",0)==0 && errno==ENOENT,15);
    char error_buffer[64];errno=71;
    CHECK(strerror_r(EINVAL,error_buffer,sizeof error_buffer)==0 && !strcmp(error_buffer,"Invalid argument") && errno==71,16);
    CHECK(strerror_r(EINVAL,error_buffer,3)==ERANGE && !strcmp(error_buffer,"In") && errno==71,17);
    CHECK(strerror_r(99999,error_buffer,sizeof error_buffer)==0 && !strcmp(error_buffer,"No error information"),18);
    CHECK(strerror_r(EINVAL,0,0)==ERANGE && errno==71,19);
    return 0;
}
void *locale_create(void) {return newlocale(LC_ALL_MASK,"C.UTF-8",0);}
int locale_validate_owner(void *handle) {return uselocale((locale_t)handle)?1:0;}
void locale_release(void) {uselocale(LC_GLOBAL_LOCALE);}
int locale_free(void *handle) {errno=71;freelocale((locale_t)handle);return errno;}
void *locale_convention(void) {return localeconv();}
#ifdef __cplusplus
}
#endif
