#include "wscan_file.h"
struct edg_wscan_string {const wchar_t *position;};
static int string_read(void *context){struct edg_wscan_string *source=(struct edg_wscan_string*)context;int value=*source->position;if(!value)return -1;++source->position;return value;}
static int string_unread(int value,void *context){struct edg_wscan_string *source=(struct edg_wscan_string*)context;--source->position;return value;}
static int file_read(void *context){return (int)fgetwc((FILE*)context);}
static int file_unread(int value,void *context){return (int)ungetwc((wint_t)value,(FILE*)context);}
static int file_body(void *context,const wchar_t *format,va_list arguments)
{struct edg_wscan_input input={file_read,file_unread,context};return __edg_musl_vwscan(&input,format,arguments);}
int vfwscanf(FILE *file,const wchar_t *format,va_list arguments)
{return __edg_with_file_wide_read_lock(file,file_body,file,format,arguments);}
int vswscanf(const wchar_t *text,const wchar_t *format,va_list arguments)
{struct edg_wscan_string source={text};struct edg_wscan_input input={string_read,string_unread,&source};return __edg_musl_vwscan(&input,format,arguments);}
int vwscanf(const wchar_t *format,va_list arguments){return vfwscanf(stdin,format,arguments);}
int fwscanf(FILE *file,const wchar_t *format,...)
{va_list arguments;va_start(arguments,format);int result=vfwscanf(file,format,arguments);va_end(arguments);return result;}
int swscanf(const wchar_t *text,const wchar_t *format,...)
{va_list arguments;va_start(arguments,format);int result=vswscanf(text,format,arguments);va_end(arguments);return result;}
int wscanf(const wchar_t *format,...)
{va_list arguments;va_start(arguments,format);int result=vfwscanf(stdin,format,arguments);va_end(arguments);return result;}
