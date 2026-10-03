#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
static int fail_malloc;
void *malloc(size_t size){if(fail_malloc){errno=ENOMEM;return 0;}return calloc(1,size);}
void *realloc(void *value,size_t size){(void)value;(void)size;errno=ENOMEM;return 0;}
int main(void){
 char *text=0;fail_malloc=1;errno=0;
 if(sscanf("abc","%ms",&text)!=EOF||errno!=ENOMEM||text)return 1;
 fail_malloc=0;errno=0;
 if(sscanf("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ","%ms",&text)!=EOF||errno!=ENOMEM||text)return 2;
 int value=0;errno=0;
 if(sscanf("17 abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ","%d%ms",&value,&text)!=1||value!=17||errno!=ENOMEM||text)return 3;
 return 0;
}
