#include "wide_numeric.h"
float wcstof_l(const wchar_t *s,wchar_t **end,locale_t locale)
{
 if(!__edg_locale_validate(locale)){if(end)*end=(wchar_t*)s;return 0;}
 return wcstof(s,end);
}
double wcstod_l(const wchar_t *s,wchar_t **end,locale_t locale)
{
 if(!__edg_locale_validate(locale)){if(end)*end=(wchar_t*)s;return 0;}
 return wcstod(s,end);
}
long double wcstold_l(const wchar_t *s,wchar_t **end,locale_t locale)
{
 if(!__edg_locale_validate(locale)){if(end)*end=(wchar_t*)s;return 0;}
 return wcstold(s,end);
}
long wcstol_l(const wchar_t *s,wchar_t **end,int base,locale_t locale)
{
 if(!__edg_locale_validate(locale)){if(end)*end=(wchar_t*)s;return 0;}
 return wcstol(s,end,base);
}
unsigned long wcstoul_l(const wchar_t *s,wchar_t **end,int base,locale_t locale)
{
 if(!__edg_locale_validate(locale)){if(end)*end=(wchar_t*)s;return 0;}
 return wcstoul(s,end,base);
}
long long wcstoll_l(const wchar_t *s,wchar_t **end,int base,locale_t locale)
{
 if(!__edg_locale_validate(locale)){if(end)*end=(wchar_t*)s;return 0;}
 return wcstoll(s,end,base);
}
unsigned long long wcstoull_l(const wchar_t *s,wchar_t **end,int base,locale_t locale)
{
 if(!__edg_locale_validate(locale)){if(end)*end=(wchar_t*)s;return 0;}
 return wcstoull(s,end,base);
}
