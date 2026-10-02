/* Supported explicit locale handles all use C numeric grammar. Validation is
   owner-scoped; no ambient/thread locale is changed by an _l conversion. */
#include <stdlib.h>
#include <inttypes.h>
#include <locale.h>
#ifdef __cplusplus
extern "C" {
#endif
int __edg_locale_validate(locale_t);
#ifdef __cplusplus
}
#endif
float strtof_l(const char *s,char **end,locale_t locale)
{if(!__edg_locale_validate(locale)){if(end)*end=(char*)s;return 0;}return strtof(s,end);}
double strtod_l(const char *s,char **end,locale_t locale)
{if(!__edg_locale_validate(locale)){if(end)*end=(char*)s;return 0;}return strtod(s,end);}
long double strtold_l(const char *s,char **end,locale_t locale)
{if(!__edg_locale_validate(locale)){if(end)*end=(char*)s;return 0;}return strtold(s,end);}
long strtol_l(const char *s,char **end,int base,locale_t locale)
{if(!__edg_locale_validate(locale)){if(end)*end=(char*)s;return 0;}return strtol(s,end,base);}
unsigned long strtoul_l(const char *s,char **end,int base,locale_t locale)
{if(!__edg_locale_validate(locale)){if(end)*end=(char*)s;return 0;}return strtoul(s,end,base);}
long long strtoll_l(const char *s,char **end,int base,locale_t locale)
{if(!__edg_locale_validate(locale)){if(end)*end=(char*)s;return 0;}return strtoll(s,end,base);}
unsigned long long strtoull_l(const char *s,char **end,int base,locale_t locale)
{if(!__edg_locale_validate(locale)){if(end)*end=(char*)s;return 0;}return strtoull(s,end,base);}
