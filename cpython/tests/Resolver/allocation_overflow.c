#include <stdlib.h>
#include <stdint.h>
#include <stdio.h>
#include <errno.h>
int main(void) {
    void *(*volatile allocate)(size_t,size_t)=calloc;
    size_t huge=SIZE_MAX; errno=123;
    void *p=allocate(huge,2); int error=errno;
    printf("ALLOCATION_OVERFLOW null=%d errno=%d\n",p==NULL,error);
    free(p); return p==NULL && error==ENOMEM?0:1;
}
