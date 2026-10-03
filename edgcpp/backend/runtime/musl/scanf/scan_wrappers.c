#include "scan_file.h"
struct edg_scan_string {const unsigned char *position;};
static int string_read(void *context)
{
 struct edg_scan_string *input=(struct edg_scan_string*)context;
 int c=*input->position;if(!c)return EOF;++input->position;return c;
}
static int file_read(void *context){return fgetc((FILE*)context);}
static int file_scan_body(void *context,const char *format,va_list arguments)
{
 struct edg_scan_input input={0,0,0,0,0,file_read,context};
 struct edg_string_scan cursor;sh_frominput(&cursor,&input);
 int result=__edg_musl_vscan(&cursor,format,arguments);
 if(input.pending)ungetc(input.last,(FILE*)context);
 return result;
}
int vfscanf(FILE *file,const char *format,va_list arguments)
{return __edg_with_file_read_lock(file,file_scan_body,file,format,arguments);}
int vsscanf(const char *text,const char *format,va_list arguments)
{
 struct edg_scan_string text_input={(const unsigned char*)text};
 struct edg_scan_input input={0,0,0,0,0,string_read,&text_input};
 struct edg_string_scan cursor;sh_frominput(&cursor,&input);
 return __edg_musl_vscan(&cursor,format,arguments);
}
int vscanf(const char *format,va_list arguments){return vfscanf(stdin,format,arguments);}
int sscanf(const char *text,const char *format,...)
{va_list arguments;va_start(arguments,format);int result=vsscanf(text,format,arguments);va_end(arguments);return result;}
int fscanf(FILE *file,const char *format,...)
{va_list arguments;va_start(arguments,format);int result=vfscanf(file,format,arguments);va_end(arguments);return result;}
int scanf(const char *format,...)
{va_list arguments;va_start(arguments,format);int result=vfscanf(stdin,format,arguments);va_end(arguments);return result;}
