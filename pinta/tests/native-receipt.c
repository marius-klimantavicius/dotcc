#include "pinta.h"
#include <stdio.h>
#include <stdlib.h>
void *pinta_platform_file_open(void *, void *, uint32_t);
uint32_t pinta_platform_file_size(void *, void *);
uint32_t pinta_platform_file_read(void *, void *, void *, uint32_t);
void pinta_platform_file_close(void *, void *);
static PintaApiString text(wchar *value) {
    PintaApiString result = {value, 0, PINTA_API_ENCODING_UTF16};
    while(value[result.string_length]) result.string_length++;
    return result;
}
int main(int argc, char **argv) {
    wchar *fixture=PINTA_STRING("receipt.pint");
    if(argc==2 && strcmp(argv[1],"receipt-swapped.pint")==0)fixture=PINTA_STRING("receipt-swapped.pint");
    else if(argc!=1)return 2;
    PintaApiEnvironment environment={0};
    environment.memory_length=4*1024*1024;
    environment.memory=calloc(1,environment.memory_length);
    environment.heap_length=1024*1024;environment.stack_length=64*1024;
    environment.platform_encoding=PINTA_API_ENCODING_UTF16;
    environment.file_open=pinta_platform_file_open;environment.file_size=pinta_platform_file_size;
    environment.file_read=pinta_platform_file_read;environment.file_close=pinta_platform_file_close;
    PintaApi *api=pinta_api_create(&environment);
    if(!api){free(environment.memory);return 2;}
    PintaApiString name=text(fixture);
    void *module=api->load_module(api,&name);
    if(!module){free(environment.memory);return 3;}
    name=text(PINTA_STRING("customer"));PintaApiString value=text(PINTA_STRING("Ada"));
    uint32_t status=api->set_string(api,module,&name,&value);
    if(status){free(environment.memory);return 4;}
    name=text(PINTA_STRING("quantity"));status=api->set_integer(api,module,&name,3);
    if(status){free(environment.memory);return 5;}
    name=text(PINTA_STRING("unitPrice"));value=text(PINTA_STRING("12.50"));status=api->set_string(api,module,&name,&value);
    if(status){free(environment.memory);return 6;}
    status=api->execute(api,module);printf("execute status=%u\n",status);
    if(status){free(environment.memory);return 7;}
    uint32_t length=0;void *data=NULL;
    status=api->unsafe_get_output_buffer(api,&length,&data);
    if(status){free(environment.memory);return 8;}
    printf("output bytes=%u hex=",length);
    for(uint32_t index=0;index<length;index++)printf("%02x",((u8*)data)[index]);
    printf("\n");
    name=text(PINTA_STRING("total"));
    status=api->unsafe_get_string(api,module,&name,PINTA_API_ENCODING_UTF16,&length,&data);
    if(status){free(environment.memory);return 9;}
    printf("total units=%u hex=",length);
    for(uint32_t index=0;index<length;index++)printf("%04x",((wchar*)data)[index]);
    printf("\n");free(environment.memory);return status!=0;
}
