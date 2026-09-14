#include "pinta.h"
#include <stdio.h>
#define TYPE(t) printf("type %s size=%zu align=%zu\n",#t,sizeof(t),_Alignof(t))
#define FIELD(t,f) printf("offset %s.%s=%zu\n",#t,#f,offsetof(t,f))
int main(void) {
    const wchar *text=PINTA_STRING("A\u017d\U0001f600");
    TYPE(void*);TYPE(long);TYPE(wchar_t);TYPE(wchar);TYPE(decimal);TYPE(PintaHeapObject);TYPE(union PintaHeapObjectData);
    TYPE(PintaReference);TYPE(PintaNativeFrame);TYPE(PintaStackFrame);TYPE(PintaModule);TYPE(PintaModuleFunction);
    TYPE(PintaApi);TYPE(PintaApiEnvironment);TYPE(PintaApiString);TYPE(PintaCore);TYPE(PintaThread);TYPE(PintaHeapCache);
    TYPE(PintaPropertyTable);TYPE(PintaPropertyValue);TYPE(PintaPropertyAccessor);TYPE(PintaPropertyNative);TYPE(PintaProperty);TYPE(PintaPropertySlot);
    FIELD(PintaHeapObject,data);FIELD(PintaNativeFrame,length);FIELD(PintaNativeFrame,next);FIELD(PintaStackFrame,return_address);
    FIELD(PintaStackFrame,function_this);FIELD(PintaModule,strings_length);FIELD(PintaModule,data_offset);
    FIELD(PintaApi,execute);FIELD(PintaApiEnvironment,file_open);FIELD(PintaApiEnvironment,platform_encoding);
    FIELD(PintaProperty,key);FIELD(PintaProperty,value);FIELD(PintaPropertyNative,native_token);FIELD(PintaPropertySlot,property_id);
    PintaPropertySlot slot={0};slot.is_valid=1;slot.is_enumerable=1;slot.is_writeable=1;slot.is_native=1;slot.property_id=0x12345678;
    printf("property-slot bytes=");for(size_t i=0;i<sizeof(slot);i++)printf("%02x",((unsigned char*)&slot)[i]);printf("\n");
    printf("literal length=%zu units=%04x,%04x,%04x,%04x,%04x char=%04x scale=%lld\n",(size_t)PINTA_LITERAL_LENGTH(PINTA_STRING("A\u017d\U0001f600")),text[0],text[1],text[2],text[3],text[4],PINTA_CHAR('\u017d'),(long long)PINTA_DECIMAL_SCALE);
    printf("stack mask=%u max_gap_bytes=%llu\n",PINTA_FRAME_PREV_OFFSET,(unsigned long long)PINTA_FRAME_PREV_OFFSET*sizeof(PintaReference));
    return !(sizeof(void*)==8 && sizeof(wchar)==2 && sizeof(wchar_t)==2 && text[1]==0x17d && text[2]==0xd83d && text[3]==0xde00 && text[4]==0);
}
