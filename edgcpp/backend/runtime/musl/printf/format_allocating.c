/* One formatting pass: geometric allocation sink. Never replay %n or arguments. */
#include "format_sink.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <stdint.h>

struct edg_allocating_buffer {char *data;size_t length,capacity;};
static int edg_allocating_write(void *context,const char *bytes,size_t count)
{
    struct edg_allocating_buffer *buffer=(struct edg_allocating_buffer *)context;
    if(!count)return 0;
    if(count>SIZE_MAX-buffer->length-1) {errno=EOVERFLOW;return -1;}
    size_t required=buffer->length+count+1;
    if(required>buffer->capacity) {
        size_t capacity=buffer->capacity?buffer->capacity:128;
        while(capacity<required)capacity=capacity>SIZE_MAX/2?required:capacity*2;
        char *grown=(char *)realloc(buffer->data,capacity);
        if(!grown)return -1;
        buffer->data=grown;buffer->capacity=capacity;
    }
    if(count)memcpy(buffer->data+buffer->length,bytes,count);
    buffer->length+=count;buffer->data[buffer->length]=0;return 0;
}
int vasprintf(char **destination,const char *format,va_list arguments)
{
    struct edg_allocating_buffer buffer={0,0,0};
    struct edg_format_sink sink={edg_allocating_write,&buffer,0};
    *destination=0;
    int result=__edg_musl_vformat(&sink,format,arguments);
    if(result<0) {free(buffer.data);return -1;}
    if(!buffer.data) {buffer.data=(char *)malloc(1);if(!buffer.data)return -1;buffer.data[0]=0;}
    *destination=buffer.data;return result;
}
int asprintf(char **destination,const char *format,...)
{
    va_list arguments;va_start(arguments,format);
    int result=vasprintf(destination,format,arguments);va_end(arguments);return result;
}
