#include "wformat_sink.h"
#include <limits.h>
#include <string.h>
/* Narrow numeric output is ASCII under the bounded C/POSIX/C.UTF-8 profile.
   Convert fixed stack chunks; never allocate a temporary numeric string. */
static int numeric_write(void *context,const char *bytes,size_t count) {
 struct edg_wformat_sink *sink=(struct edg_wformat_sink *)context;
 wchar_t chunk[64];
 while(count) {size_t n=count<64?count:64;for(size_t i=0;i<n;++i)chunk[i]=(unsigned char)bytes[i];
  if(edg_wformat_write(sink,chunk,n)<0)return -1;bytes+=n;count-=n;}
 return 0;
}
int edg_wformat_narrow(struct edg_wformat_sink *sink,const char *format,...) {
 struct edg_format_sink narrow={numeric_write,sink,0};va_list arguments;va_start(arguments,format);
 int result=__edg_musl_vformat(&narrow,format,arguments);va_end(arguments);return result;
}
static int file_write(void *context,const wchar_t *text,size_t count) {
 FILE *file=(FILE *)context;
 while(count--)if(fputwc(*text++,file)==WEOF)return -1;
 return 0;
}
int vfwprintf(FILE *file,const wchar_t *format,va_list arguments) {
 struct edg_wformat_sink sink={file_write,file,0};return __edg_musl_vwformat(&sink,file,format,arguments);
}
int fwprintf(FILE *file,const wchar_t *format,...) {
 va_list arguments;va_start(arguments,format);int result=vfwprintf(file,format,arguments);va_end(arguments);return result;
}
int vwprintf(const wchar_t *format,va_list arguments) {return vfwprintf(stdout,format,arguments);}
int wprintf(const wchar_t *format,...) {
 va_list arguments;va_start(arguments,format);int result=vwprintf(format,arguments);va_end(arguments);return result;
}
struct wide_buffer {wchar_t *next;size_t left;};
static int buffer_write(void *context,const wchar_t *text,size_t count) {
 struct wide_buffer *buffer=(struct wide_buffer *)context;
 for(size_t i=0;i<count;++i) {
  /* Preserve the original memory FILE's scalar/locale encoding failures while
     storing the exact wide scalar directly, without a transcode allocation. */
  char bytes[MB_LEN_MAX];mbstate_t state={0};
  if(wcrtomb(bytes,text[i],&state)==(size_t)-1){*buffer->next=0;return -1;}
  if(buffer->left){*buffer->next++=text[i];--buffer->left;}
 }
 *buffer->next=0;return 0;
}
int vswprintf(wchar_t *destination,size_t capacity,const wchar_t *format,va_list arguments) {
 if(!capacity)return -1;
 *destination=0;struct wide_buffer buffer={destination,capacity-1};
 struct edg_wformat_sink sink={buffer_write,&buffer,0};int result=__edg_musl_vwformat(&sink,0,format,arguments);
 return result<0||(size_t)result>=capacity?-1:result;
}
int swprintf(wchar_t *destination,size_t capacity,const wchar_t *format,...) {
 va_list arguments;va_start(arguments,format);int result=vswprintf(destination,capacity,format,arguments);va_end(arguments);return result;
}
