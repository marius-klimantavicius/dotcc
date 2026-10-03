/* Authored owner-environment selection, following musl's LC_ALL/category/LANG
   precedence and C.UTF-8 fallback. Unsupported names fail without mutation. */
#include <locale.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#ifdef __cplusplus
extern "C" {
#endif
char *__edg_locale_set_name(int,const char *);
locale_t __edg_locale_new_name(int,const char *,locale_t);
char *__edg_locale_set_categories(int,int);
locale_t __edg_locale_new_categories(int,int,locale_t);
static const char *const category_names[6]={"LC_CTYPE","LC_NUMERIC","LC_TIME","LC_COLLATE","LC_MONETARY","LC_MESSAGES"};
static int resolve_categories(int mask)
{
 int result=0;
 const char *all=getenv("LC_ALL");
 for(int category=0;category<6;++category)if(mask&(1<<category)) {
  const char *name=all;
  if(!name || !*name)name=getenv(category_names[category]);
  if(!name || !*name)name=getenv("LANG");
  if(!name || !*name)name="C.UTF-8";
  if(!strcmp(name,"C") || !strcmp(name,"POSIX"))continue;
  if(strcmp(name,"C.UTF-8") && strcmp(name,"C.utf8"))return -1;
  result|=1<<category;
 }
 return result;
}
char *setlocale(int category,const char *name)
{
 int values;
 if(!name || *name)return __edg_locale_set_name(category,name);
 if((unsigned)category>LC_ALL){errno=EINVAL;return 0;}
 values=resolve_categories(category==LC_ALL?LC_ALL_MASK:1<<category);
 if(values<0){errno=EINVAL;return 0;}
 return __edg_locale_set_categories(category,values);
}
locale_t newlocale(int mask,const char *name,locale_t basis)
{
 int values;
 if(!name || *name || !mask)return __edg_locale_new_name(mask,name,basis);
 if(mask&~LC_ALL_MASK){errno=EINVAL;return 0;}
 values=resolve_categories(mask);
 if(values<0){errno=ENOENT;return 0;}
 return __edg_locale_new_categories(mask,values,basis);
}
#ifdef __cplusplus
}
#endif
