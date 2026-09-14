#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#include <string.h>
#include <fcntl.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_DESCRIPTORS
#include "host-io.h"
#include "host-descriptors.h"
#define ROOT ""
#endif
#define CHECK(x) do{if(!(x)){printf("descriptor replacement failure %d\n",__LINE__);return __LINE__;}}while(0)
int DescriptorProbe(void) {
  char bytes[8];int fd=open(ROOT "/source",O_CREAT|O_RDWR|O_EXCL,0600);CHECK(fd>=0);
  int target=open(ROOT "/target",O_CREAT|O_RDWR|O_EXCL,0600);CHECK(target>=0);
  CHECK(write(fd,"abcdef",6)==6 && write(target,"old",3)==3);
  CHECK(!fcntl(fd,F_SETFD,FD_CLOEXEC));CHECK(dup2(fd,fd)==fd && fcntl(fd,F_GETFD)==FD_CLOEXEC);
  CHECK(dup3(fd,fd,0)==-1 && errno==EINVAL);
  CHECK(dup3(fd,target,123)==-1 && errno==EINVAL && lseek(target,0,SEEK_CUR)==3);
  CHECK(dup2(-1,target)==-1 && errno==EBADF && lseek(target,0,SEEK_CUR)==3);
  errno=123;CHECK(dup2(fd,target)==target && errno==123 && fcntl(target,F_GETFD)==0);
  CHECK(lseek(target,1,SEEK_SET)==1 && read(fd,bytes,2)==2 && !memcmp(bytes,"bc",2));
  CHECK(dup3(fd,target,O_CLOEXEC)==target && fcntl(target,F_GETFD)==FD_CLOEXEC && lseek(target,0,SEEK_CUR)==3);
  int (*replace)(int,int)=dup2;CHECK(replace(fd,7)==7 && fcntl(7,F_GETFD)==0);
  CHECK(!close(fd) && !close(target) && read(7,bytes,3)==3 && !memcmp(bytes,"def",3));CHECK(!close(7));
  target=open(ROOT "/target",O_RDONLY);CHECK(target>=0 && read(target,bytes,3)==3 && !memcmp(bytes,"old",3));CHECK(!close(target));
  CHECK(dup2(-1,-1)==-1 && errno==EBADF);
  CHECK(dup3(-1,-1,0)==-1 && errno==EINVAL);
  puts("descriptor replacement: atomic target, shared cursor, last close, independent flags: PASS");return 0;
}
#ifdef BLINK_MANAGED_DESCRIPTORS
int DescriptorPrivate(void) {
  int a=open("/a",O_CREAT|O_RDWR,0600),b=open("/b",O_CREAT|O_RDWR,0600),c=open("/c",O_CREAT|O_RDWR,0600);CHECK(a>=0 && b>=0 && c>=0);
  CHECK(dup(a)==-1 && errno==EMFILE);
  CHECK(dup2(a,c)==c && dup3(a,b,O_CLOEXEC)==b);
  CHECK(dup2(a,6)==-1 && errno==EBADF);
  CHECK(write(a,"q",1)==1 && lseek(b,0,SEEK_CUR)==1 && lseek(c,0,SEEK_CUR)==1);
  CHECK(!close(a) && !close(b) && lseek(c,0,SEEK_SET)==0);char q;CHECK(read(c,&q,1)==1 && q=='q');CHECK(!close(c));return 0;
}
int DescriptorUnbound(void){CHECK(dup2(0,1)==-1 && errno==ENODEV);return 0;}
#else
int main(void){return DescriptorProbe();}
#endif
