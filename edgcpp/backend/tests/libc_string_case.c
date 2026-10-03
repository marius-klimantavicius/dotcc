#include <strings.h>
#include <locale.h>
#include <stddef.h>
#ifdef __cplusplus
extern "C" {
#endif
static int folded(unsigned char value) {return value>='A'&&value<='Z'?value+32:value;}
int string_case_cases(void) {
  locale_t locale=newlocale(LC_ALL_MASK,"C",(locale_t)0);
  if(!locale)return 1;
  for(unsigned a=1;a<256;++a)for(unsigned b=1;b<256;++b) {
    char left[3]={(char)a,'Q',0},right[3]={(char)b,'q',0};
    int expected=folded((unsigned char)a)-folded((unsigned char)b);
    if(strcasecmp(left,right)!=expected||strncasecmp(left,right,1)!=expected||strncasecmp(left,right,2)!=expected)return 2;
    if(strcasecmp_l(left,right,locale)!=expected||strncasecmp_l(left,right,1,locale)!=expected)return 3;
  }
  if(strncasecmp((const char*)0,(const char*)0,0)!=0)return 4;
  if(strncasecmp_l((const char*)0,(const char*)0,0,locale)!=0)return 5;
  if(strcasecmp("AbC","aBc")||strncasecmp("AbCd","aBCx",3))return 6;
  if(strcasecmp("abc","ABCD")!='\0'-'d'||strncasecmp("ABC","abcd",4)!='\0'-'d')return 7;
  freelocale(locale);return 0;
}
#ifdef __cplusplus
}
#endif
