#define _GNU_SOURCE
#include <locale.h>
#include <stdlib.h>
#include <string.h>
#include <sys/mman.h>
#include <stdio.h>
#include "locale_impl.h"
#include "libc.h"
struct test_libc libc={1};
const struct __locale_map __c_dot_utf8={0,0,"C.UTF-8",0};
const char *__mo_lookup(const void*p,size_t n,const char*s){(void)p;(void)n;(void)s;return 0;}
const void *__map_file(const char*p,size_t*n){(void)p;(void)n;return 0;}
char *__strchrnul(const char*p,int c){return strchrnul(p,c);}
void *__libc_malloc(size_t n){return malloc(n);}
int __munmap(void*p,size_t n){return munmap(p,n);}
int main(void){
 const char *names[]={0,"","C","POSIX","C.UTF-8"};unsigned count=0;
 for(int a=0;a<5;++a)for(int b=0;b<5;++b)for(int c=0;c<5;++c){
  if(names[a])setenv("LC_ALL",names[a],1);else unsetenv("LC_ALL");
  if(names[b])setenv("LC_CTYPE",names[b],1);else unsetenv("LC_CTYPE");
  if(names[c])setenv("LANG",names[c],1);else unsetenv("LANG");
  const char *chosen=names[a]&&*names[a]?names[a]:names[b]&&*names[b]?names[b]:names[c]&&*names[c]?names[c]:"C.UTF-8";
  int expected=!strcmp(chosen,"C.UTF-8");
  if((__get_locale(LC_CTYPE,"")!=0)!=expected)return 1;++count;
 }
 printf("original musl LC_CTYPE precedence/fallback combinations=%u\n",count);return 0;
}
