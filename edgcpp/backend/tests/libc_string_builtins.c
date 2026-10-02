#include <string.h>
#include <stddef.h>
static int same_address(void *left,void *right){return left==right;}
int main(void)
{
    char a[32],b[32];const char *source="abcabc";
    if(__builtin_memset(a,0,sizeof(a))!=a || __builtin_memcpy(a,source,7)!=a)return 1;
    if(__builtin_memcmp(a,source,7) || __builtin_memchr(a,'b',7)!=a+1)return 2;
    if(__builtin_memmove(a+1,a,7)!=a+1 || __builtin_strcmp(a,"aabcabc"))return 3;
    if(__builtin_strlen(a)!=7 || __builtin_strnlen(a,3)!=3 || __builtin_strncmp(a,"aabd",3))return 4;
    if(__builtin_strcpy(b,"ab")!=b || __builtin_strcat(b,"cd")!=b || __builtin_strcmp(b,"abcd"))return 5;
    if(__builtin_strncpy(b,"q",4)!=b || b[1] || b[2] || b[3])return 6;
    if(__builtin_strncat(b,"RST",2)!=b || __builtin_strcmp(b,"qRS"))return 7;
    if(__builtin_strchr(a,'c')!=a+3 || __builtin_strrchr(a,'c')!=a+6 || __builtin_strstr(a,"bca")!=a+2)return 8;
    if(__builtin_strspn(a,"ab")!=3 || __builtin_strcspn(a,"cx")!=3 || __builtin_strpbrk(a,"cx")!=a+3)return 9;
    if(!same_address((void*)memcpy,(void*)__builtin_memcpy) || !same_address((void*)memcmp,(void*)__builtin_memcmp) || !same_address((void*)strlen,(void*)__builtin_strlen))return 10;
    return 0;
}
