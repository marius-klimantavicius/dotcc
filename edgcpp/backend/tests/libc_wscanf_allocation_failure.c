#include <stdio.h>
#include <wchar.h>
#include <stdlib.h>
#include <errno.h>
static int fail_malloc;
void *malloc(size_t size){if(fail_malloc){errno=ENOMEM;return 0;}return calloc(1,size);}
void *realloc(void *value,size_t size){(void)value;(void)size;errno=ENOMEM;return 0;}
int main(void){
 char *text=0;fail_malloc=1;errno=0;
 if(swscanf(L"abc",L"%ms",&text)!=EOF||errno!=ENOMEM||text)return 1;
 fail_malloc=0;errno=0;
 if(swscanf(L"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ",L"%ms",&text)!=EOF||errno!=ENOMEM||text)return 2;
 int value=0;errno=0;
 if(swscanf(L"17 abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ",L"%d%ms",&value,&text)!=1||value!=17||errno!=ENOMEM||text)return 3;
 wchar_t *wide=0;fail_malloc=1;errno=0;
 if(swscanf(L"abc",L"%mls",&wide)!=EOF||errno!=ENOMEM||wide)return 4;
 fail_malloc=0;errno=0;
 if(swscanf(L"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ",L"%mls",&wide)!=EOF||errno!=ENOMEM||wide)return 5;
 return 0;
}
