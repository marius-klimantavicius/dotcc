#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>
static void *(*allocfn)(size_t)=malloc;
static void *(*zerofn)(size_t,size_t)=calloc;
static void *(*resizefn)(void *,size_t)=realloc;
int main(void) {
    unsigned char *data=allocfn(16);
    unsigned char *zeros=zerofn(4,4);
    if(!data || !zeros) return 1;
    data[0]=43; data[15]=71;
    int zero=0; for(int i=0;i<16;i++) zero += zeros[i];
    data=resizefn(data,32);
    printf("%d %d %d\n",data[0],data[15],zero);
    size_t impossible=SIZE_MAX-127;
    void *failure=resizefn(data,impossible);
    printf("%d %d %d\n",failure==NULL,data[0],data[15]);
    printf("%d %d\n",allocfn(impossible)==NULL,zerofn(SIZE_MAX,2)==NULL);
    free(data);free(zeros);
    return 0;
}
