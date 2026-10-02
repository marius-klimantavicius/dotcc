/* Synthetic public-wrapper checks; expectations are fixed, not self-oracles. */
#include <stdio.h>
#include <stdarg.h>
#include <string.h>
static int buffer_vcall(char *out, size_t cap, const char *format, ...)
{
    va_list args;va_start(args,format);
    int result=cap?vsnprintf(out,cap,format,args):vsprintf(out,format,args);
    va_end(args);return result;
}
static int file_vcall(FILE *file, const char *format, ...)
{
    va_list args;va_start(args,format);int result=vfprintf(file,format,args);va_end(args);return result;
}
static int print_vcall(const char *format, ...)
{
    va_list args;va_start(args,format);int result=vprintf(format,args);va_end(args);return result;
}
int main(int argc, char **argv)
{
    if(argc!=2)return 1;
    char buffer[128];
    if(snprintf(buffer,sizeof buffer,"%05d|%s|%.2f",-42,"xy",1.25)!=13 || strcmp(buffer,"-0042|xy|1.25"))return 2;
    if(sprintf(buffer,"x%d",3)!=2 || strcmp(buffer,"x3"))return 3;
    if(buffer_vcall(buffer,5,"abcdef%d",7)!=7 || strcmp(buffer,"abcd"))return 4;
    if(buffer_vcall(buffer,0,"v%d",8)!=2 || strcmp(buffer,"v8"))return 5;
    if(snprintf(0,0,"count:%d",18)!=8)return 6;
    FILE *file=fopen(argv[1],"w+b");if(!file)return 7;
    if(fprintf(file,"f:%d\n",7)!=4 || file_vcall(file,"v:%s\n","q")!=4)return 8;
    FILE *saved=stdout;stdout=file;
    int one=printf("p:%u\n",8u),two=print_vcall("w:%c\n",'R');stdout=saved;
    if(one!=4 || two!=4 || fseek(file,0,SEEK_SET))return 9;
    size_t bytes=fread(buffer,1,sizeof buffer-1,file);buffer[bytes]=0;
    if(bytes!=16 || strcmp(buffer,"f:7\nv:q\np:8\nw:R\n"))return 10;
    return fclose(file)!=0;
}
