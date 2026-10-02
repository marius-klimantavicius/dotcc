#include <stdio.h>
#include <stdlib.h>
#include <locale.h>
#include <wchar.h>
#include <errno.h>
#include <stdint.h>
#include <limits.h>
static void vector(int value) {
 unsigned char bytes[16];for(int i=0;i<16;++i)bytes[i]=0xa5;
 errno=0;int length=wctomb((char*)bytes,(wchar_t)value);
 printf("%d %d %d %02x%02x%02x%02x\n",value,length,errno,bytes[0],bytes[1],bytes[2],bytes[3]);
}
int main(void) {
 if(!setlocale(LC_CTYPE,"C.UTF-8"))return 1;
 int boundary[]={INT_MIN,-1,0,1,0x7f,0x80,0x7ff,0x800,0xd7ff,0xd800,0xdbff,0xdc00,0xdfff,0xe000,0xffff,0x10000,0x10ffff};
 for(unsigned i=0;i<sizeof boundary/sizeof*boundary;++i)vector(boundary[i]);
 uint32_t state=19281732;
 for(int i=0;i<3000;++i){state=state*1664525+1013904223;vector(state%0x110000);}
 return 0;
}
