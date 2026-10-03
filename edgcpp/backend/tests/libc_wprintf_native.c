#include <stdio.h>
#include <stdlib.h>
#include <wchar.h>
#include <stdint.h>
#ifndef WPRINTF_CASE
#define WPRINTF_CASE wprintf_case
#endif
int wprintf_file_case(const char*);int wprintf_activate(void);void WPRINTF_CASE(int,const wchar_t*,unsigned,void*);
static unsigned digit(char c){return c>='a'?c-'a'+10:c-'0';}
int main(int argc,char**argv){if((argc!=3&&argc!=4)||!wprintf_activate())return 3;FILE*in=fopen(argv[1],"r"),*out=fopen(argv[2],"wb");if(!in||!out)return 4;
 int kind;unsigned capacity;char hex[8192];while(fscanf(in,"%d %u %8191s",&kind,&capacity,hex)==3){wchar_t format[1024];unsigned i=0;while(hex[i*8]){uint32_t value=0;for(unsigned j=0;j<4;++j)value|=(digit(hex[i*8+j*2])*16+digit(hex[i*8+j*2+1]))<<(j*8);format[i++]=(wchar_t)value;}format[i]=0;unsigned char bytes[1040];WPRINTF_CASE(kind,format,capacity,bytes);if(fwrite(bytes,1,1040,out)!=1040)return 5;}int result=argc==4?wprintf_file_case(argv[3]):0;return result?30+result:fclose(in)||fclose(out);}
