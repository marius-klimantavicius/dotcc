#include <locale.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#ifdef __cplusplus
extern "C" {
#endif
int __edg_locale_categories(locale_t);
static const char *const keys[6]={"LC_CTYPE","LC_NUMERIC","LC_TIME","LC_COLLATE","LC_MONETARY","LC_MESSAGES"};
static const char *const options[5]={0,"","C","POSIX","C.UTF-8"};
static void variable(const char *key,const char *value){if(value)setenv(key,value,1);else unsetenv(key);}
int locale_environment_contract(void)
{
 int category,a,b,c,selected;locale_t base,thread;
 char *saved;
 clearenv();
 if(!setlocale(LC_ALL,"C") || !setlocale(LC_ALL,"") || __edg_locale_categories(0)!=63)return 1;
 for(category=0;category<6;++category)for(a=0;a<5;++a)for(b=0;b<5;++b)for(c=0;c<5;++c) {
  variable("LC_ALL",options[a]);variable(keys[category],options[b]);variable("LANG",options[c]);
  selected=a>=2?a:b>=2?b:c>=2?c:4;
  if(!setlocale(LC_ALL,"C"))return 2;
  errno=73;
  if(!setlocale(category,"") || __edg_locale_categories(0)!=(selected==4?1<<category:0) || errno!=73)return 3;
  variable(keys[category],0);
 }
 clearenv();setenv("LANG","C",1);setenv("LC_CTYPE","C.UTF-8",1);setenv("LC_TIME","C.utf8",1);
 saved=setlocale(LC_ALL,"");if(!saved || __edg_locale_categories(0)!=5 || strcmp(saved,"C.UTF-8;C;C.UTF-8;C;C;C"))return 4;
 setenv("LC_MESSAGES","unsupported-owner-locale",1);errno=0;
 if(setlocale(LC_ALL,"") || errno!=EINVAL || __edg_locale_categories(0)!=5 || strcmp(saved,"C.UTF-8;C;C.UTF-8;C;C;C"))return 5;
 if(!setlocale(LC_NUMERIC,"") || __edg_locale_categories(0)!=5)return 6;
 setenv("LC_ALL","C.UTF-8",1);if(!setlocale(LC_ALL,"") || __edg_locale_categories(0)!=63)return 7;
 setenv("LC_ALL","C;C;C;C;C;C",1);if(setlocale(LC_ALL,"") || __edg_locale_categories(0)!=63)return 8;
 clearenv();setenv("LANG","C",1);setenv("LC_TIME","C.UTF-8",1);
 base=newlocale(LC_CTYPE_MASK,"C.UTF-8",0);if(!base || __edg_locale_categories(base)!=1)return 9;
 if(newlocale(LC_TIME_MASK,"",base)!=base || __edg_locale_categories(base)!=5)return 10;
 setenv("LC_CTYPE","invalid",1);errno=0;
 if(newlocale(LC_ALL_MASK,"",base) || errno!=ENOENT || __edg_locale_categories(base)!=5)return 11;
 if(newlocale(0,"",base)!=base || __edg_locale_categories(base)!=5)return 12;
 if(uselocale(base)!=LC_GLOBAL_LOCALE || MB_CUR_MAX!=4)return 13;
 errno=0;if(newlocale(LC_TIME_MASK,"",base) || errno!=EBUSY || __edg_locale_categories(base)!=5)return 14;
 if(!setlocale(LC_ALL,"C") || MB_CUR_MAX!=4)return 15;
 uselocale(LC_GLOBAL_LOCALE);if(MB_CUR_MAX!=1)return 16;
 freelocale(base);clearenv();setenv("LANG","C.UTF-8",1);
 thread=newlocale(LC_TIME_MASK,"",0);if(!thread || __edg_locale_categories(thread)!=4)return 17;
 freelocale(thread);
 errno=0;if(newlocale(64,"",0) || errno!=EINVAL)return 18;
 errno=0;if(setlocale(-1,"") || errno!=EINVAL)return 19;
 if(!setlocale(LC_ALL,"C"))return 20;
 return 0;
}
int locale_environment_set(const char *name,const char *value){return setenv(name,value,1);}
int locale_environment_select(void){return setlocale(LC_ALL,"")?__edg_locale_categories(0):-1;}
void locale_environment_clear(void){clearenv();setlocale(LC_ALL,"C");}
int locale_environment_categories(void){return __edg_locale_categories(0);}
int locale_environment_repeat(void){int i;for(i=0;i<10000;++i)if(!setlocale(LC_ALL,""))return -1;return __edg_locale_categories(0);}
#ifdef __cplusplus
}
#endif
