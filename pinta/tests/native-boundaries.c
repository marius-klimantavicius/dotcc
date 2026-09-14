#include "pinta.h"
#include <stdio.h>
#include <stdlib.h>
int main(int argc,char **argv) {
    if(argc!=2)return 2;
    unsigned char *raw=malloc(131072+128);if(!raw)return 2;
    uintptr_t address=((uintptr_t)raw+63)&~(uintptr_t)63;
    unsigned char *arena=(unsigned char*)address;
    if(strcmp(argv[1],"misaligned-short")==0) {
        int failures=0;
        for(unsigned offset=1;offset<64;offset++) for(unsigned length=0;length<64-offset;length++) {
            memset(arena,0xa5,128);PintaNativeMemory *memory=pinta_memory_init(arena+offset,length);
            if(memory)failures++;
            for(unsigned i=0;i<128;i++)if(arena[i]!=0xa5){failures++;break;}
        }
        printf("misaligned-short failures=%d\n",failures);free(raw);return failures?1:0;
    }
    if(strcmp(argv[1],"create-sweep")==0) {
        unsigned pass=0,fail=0;
        for(unsigned length=0;length<=65536;length+=16) {
            PintaApiEnvironment env={0};env.memory=arena;env.memory_length=length;env.heap_length=1024;env.stack_length=128;
            memset(arena,0xa5,131072);
            PintaApi *api=pinta_api_create(&env);if(api)pass++;else fail++;
            for(unsigned i=length;i<131072;i++)if(arena[i]!=0xa5){printf("canary changed at length=%u offset=%u\n",length,i);free(raw);return 1;}
        }
        printf("create-sweep successes=%u rejected=%u\n",pass,fail);free(raw);return pass&&fail?0:1;
    }
    if(strcmp(argv[1],"stack-overflow")==0) {
        PintaNativeMemory *memory=pinta_memory_init(arena,65536);PintaThread thread={0};
        PintaException status=pinta_frame_init(&thread,memory,UINT32_MAX);printf("stack-overflow status=%u\n",status);free(raw);return status==PINTA_EXCEPTION_INVALID_ARGUMENTS?0:1;
    }
    free(raw);return 2;
}
