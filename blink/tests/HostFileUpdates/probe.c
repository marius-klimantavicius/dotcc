#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#include <string.h>
#include <fcntl.h>
#include <unistd.h>
#include <sys/stat.h>
#ifdef BLINK_MANAGED_UPDATES
#include "host-io.h"
#include "host-file-updates.h"
#define ROOT ""
#endif
#define CHECK(x) do {if(!(x)){printf("file update failure %d\n",__LINE__);return __LINE__;}}while(0)
int FileUpdateProbe(void) {
  char bytes[8];struct stat st;
  int fd=open(ROOT "/mutable",O_RDWR|O_CREAT|O_EXCL,0600);CHECK(fd>=0);
  CHECK(write(fd,"abcd",4)==4);int copy=dup(fd);CHECK(copy>=0);
  CHECK(pwrite(fd,"XY",2,1)==2 && lseek(copy,0,SEEK_CUR)==4);
  CHECK(!ftruncate(fd,6) && !fstat(fd,&st) && st.st_size==6);
  CHECK(pread(fd,bytes,6,0)==6 && !memcmp(bytes,"aXYd\0\0",6));
  CHECK(lseek(fd,12,SEEK_SET)==12 && !ftruncate(copy,2) && lseek(fd,0,SEEK_CUR)==12);
  CHECK(pwrite(fd,"z",1,4)==1 && pread(fd,bytes,5,0)==5 && !memcmp(bytes,"aX\0\0z",5));
  CHECK(!truncate(ROOT "/mutable",1) && lseek(fd,0,SEEK_CUR)==12);
  errno=123;CHECK(!fsync(fd) && errno==123 && !fdatasync(copy) && errno==123);
  CHECK(pwrite(fd,"",0,-1)==-1 && errno==EINVAL);
  CHECK(ftruncate(fd,-1)==-1 && errno==EINVAL);
  CHECK(!close(copy) && !close(fd));
  fd=open(ROOT "/mutable",O_WRONLY|O_APPEND);CHECK(fd>=0);
  CHECK(pwrite(fd,"Q",1,0)==1 && lseek(fd,0,SEEK_CUR)==0);CHECK(!close(fd));
  fd=open(ROOT "/mutable",O_RDONLY);CHECK(fd>=0);
  CHECK(read(fd,bytes,2)==2 && !memcmp(bytes,"aQ",2));
  CHECK(ftruncate(fd,0)==-1 && errno==EINVAL);CHECK(!fsync(fd));CHECK(!close(fd));
  CHECK(fsync(fd)==-1 && errno==EBADF);
  puts("private file updates: positioned writes, resize, zero fill, cursors, append, sync: PASS");return 0;
}
#ifdef BLINK_MANAGED_UPDATES
int FileUpdatePrivate(void) {
  struct stat st;int fd=open("/quota",O_RDWR|O_CREAT,0600);CHECK(fd>=0);
  CHECK(!ftruncate(fd,8));CHECK(ftruncate(fd,9)==-1 && errno==ENOSPC);
  CHECK(!fstat(fd,&st) && st.st_size==8);
  CHECK(pwrite(fd,"x",1,8)==-1 && errno==ENOSPC);
  CHECK(!ftruncate(fd,2));int other=open("/other",O_RDWR|O_CREAT,0600);CHECK(other>=0 && !ftruncate(other,6));
  CHECK(ftruncate(fd,3)==-1 && errno==ENOSPC && !fstat(fd,&st) && st.st_size==2);
  CHECK(truncate("/seed",0)==-1 && errno==EROFS);
  CHECK(truncate("/missing",0)==-1 && errno==ENOENT);
  CHECK(truncate("/",0)==-1 && errno==EISDIR);
  CHECK(truncate(0,0)==-1 && errno==EFAULT);
  CHECK(fsync(1)==-1 && errno==EINVAL && fdatasync(0)==-1 && errno==EINVAL);
  CHECK(!close(other) && !close(fd));return 0;
}
int FileUpdateWorker(int length) {
  int fd=open("/same",O_RDWR|O_CREAT|O_EXCL,0600);struct stat st;CHECK(fd>=0);
  int (*resize)(int,off_t)=ftruncate;CHECK(!resize(fd,length));
  CHECK(!fstat(fd,&st) && st.st_size==length);CHECK(!close(fd));return 0;
}
int FileUpdateUnbound(void){CHECK(fsync(0)==-1 && errno==ENODEV);return 0;}
#else
int main(void){return FileUpdateProbe();}
#endif
