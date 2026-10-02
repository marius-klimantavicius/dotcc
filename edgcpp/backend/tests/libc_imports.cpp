#include <stddef.h>
#include <stdint.h>
#include <string.h>
#include <stdlib.h>
#include <errno.h>
#include <math.h>

extern "C" int run_libc_case(int which) {
  char buffer[64], second[64];
  memset(buffer, 0, sizeof buffer);
  memset(second, 0, sizeof second);
  switch(which) {
    case 0: {
      void *(*copy)(void*,const void*,size_t)=memcpy;
      void *(*again)(void*,const void*,size_t)=&memcpy;
      return copy==again && copy(buffer,"abc",4)==buffer && strcmp(buffer,"abc")==0;
    }
    case 1: {
      strcpy(buffer,"abcdef");memmove(buffer+1,buffer,6);
      return memcmp(buffer,"aabcdef",7)==0;
    }
    case 2: {
      strcpy(buffer,"abcdef");memmove(buffer,buffer+1,6);
      return strcmp(buffer,"bcdef")==0;
    }
    case 3: {
      memset(buffer,0xfe,8);
      return (unsigned char)buffer[7]==254 && memchr(buffer,510,8)==buffer && memchr(buffer,1,8)==0;
    }
    case 4: {
      buffer[0]=(char)0x80;second[0]=0x7f;
      return memcmp(buffer,second,1)>0 && strcmp(buffer,second)>0 && strncmp(buffer,second,1)>0;
    }
    case 5: return strlen("abcd")==4 && strnlen("abcd",2)==2 && strnlen("ab",9)==2;
    case 6: {
      memset(buffer,0x7f,64);strncpy(buffer,"hi",8);
      return buffer[1]=='i' && buffer[2]==0 && buffer[7]==0 && buffer[8]==0x7f;
    }
    case 7: {
      strcpy(buffer,"one");strcat(buffer,"two");strncat(buffer,"three",2);
      return strcmp(buffer,"onetwoth")==0;
    }
    case 8: {
      strcpy(buffer,"abca");return strchr(buffer,'a')==buffer && strrchr(buffer,'a')==buffer+3 &&
        strchr(buffer,0)==buffer+4 && strrchr(buffer,0)==buffer+4 && strchr(buffer,'x')==0;
    }
    case 9: {
      strcpy(buffer,"ababa");return strstr(buffer,"aba")==buffer && strstr(buffer,"ba")==buffer+1 &&
        strstr(buffer,"")==buffer && strstr(buffer,"abb")==0;
    }
    case 10: return strspn("aaabxy","ab")==4 && strcspn("aaabxy","xy")==4 && strpbrk("abc","z")==0;
    case 11: {
      strcpy(buffer,",one,,two,");char *state=0;
      char *one=strtok_r(buffer,",",&state),*two=strtok_r(0,",",&state);
      return strcmp(one,"one")==0 && strcmp(two,"two")==0 && strtok_r(0,",",&state)==0;
    }
    case 12: {
      strcpy(buffer,"one,two");char *one=strtok(buffer,",");char *two=strtok(0,",");
      return strcmp(one,"one")==0 && strcmp(two,"two")==0 && strtok(0,",")==0;
    }
    case 13: {
      void *(*allocate)(size_t)=malloc;void (*release)(void*)=free;
      char *p=(char*)allocate(32);if(!p)return 0;
      strcpy(p,"preserved");p=(char*)realloc(p,128);if(!p)return 0;
      int good=strcmp(p,"preserved")==0;p=(char*)realloc(p,16);
      good=good && p && strcmp(p,"preserved")==0;release(p);return good;
    }
    case 14: {
      unsigned char *p=(unsigned char*)calloc(32,2);if(!p)return 0;
      int good=1;for(int i=0;i<64;++i)if(p[i])good=0;free(p);return good;
    }
    case 15: {
      errno=0;volatile size_t maximum=(size_t)-1;void *p=calloc(maximum,2);
      return p==0 && errno==ENOMEM;
    }
    case 16: {
      char *p=(char*)malloc(16);if(!p)return 0;p[0]=42;
      errno=0;volatile size_t maximum=(size_t)-1;void *q=realloc(p,maximum);
      int good=q==0 && errno==ENOMEM && p[0]==42;free(p);return good;
    }
    case 17: {
      void *p=aligned_alloc(64,128);int good=p && ((uintptr_t)p&63)==0;free(p);return good;
    }
    case 18: {
      void *p=(void*)123;errno=7;
      int bad=posix_memalign(&p,3,128),good=bad==EINVAL && p==(void*)123 && errno==7;
      int result=posix_memalign(&p,64,13);good=good && result==0 && ((uintptr_t)p&63)==0;
      free(p);return good;
    }
    case 19: {
      double (*fn)(double)=trunc;return fn(-3.75)==-3 && floor(-2.25)==-3 && ceil(2.25)==3 && fabs(-2.25)==2.25;
    }
    case 20: return truncf(-3.75f)==-3 && floorf(-2.25f)==-3 && ceilf(2.25f)==3 && fabsf(-2.25f)==2.25;
    case 21: {
      volatile double negative=-1;errno=0;double n=sqrt(negative);
      return n!=n && errno==EDOM && sqrt(9)==3 && sqrtf(16)==4;
    }
    case 22: {
      void *p=malloc(0);free(p);p=calloc(0,17);free(p);p=realloc(0,12);
      int good=p!=0;free(p);free(0);return good;
    }
    case 23: {
      errno=81;int *p=&errno;*p=82;return errno==82;
    }
  }
  return 0;
}
extern "C" int read_libc_errno() {return errno;}
extern "C" void write_libc_errno(int value) {errno=value;}
extern "C" int *libc_errno_address() {return &errno;}
static char tokens[32];
extern "C" int libc_token_begin(int value) {
  strcpy(tokens,value?"B,b":"A,a");return *strtok(tokens,",");
}
extern "C" int libc_token_next() {char *p=strtok(0,",");return p?*p:0;}
