#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <string.h>
#include <sys/stat.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_METADATA
#include "host-io.h"
#endif
#define CHECK(x) do { if (!(x)) { printf("failure %d\n",__LINE__); return __LINE__; } } while(0)
int MetadataProbe(void) {
  struct stat a,b,c;
  CHECK(!stat("data/image",&a));
  CHECK(a.st_size==5 && S_ISREG(a.st_mode) && (a.st_mode&0777)==0444 && a.st_ino && a.st_nlink==1);
  CHECK(!lstat("data/image",&b) && a.st_ino==b.st_ino && a.st_dev==b.st_dev);
  CHECK(!stat("data/executable",&b) && (b.st_mode&0777)==0555 && a.st_ino!=b.st_ino);
  CHECK(!stat("data",&b) && S_ISDIR(b.st_mode) && b.st_nlink>=2);
  int image=open("data/image",O_RDONLY);
  CHECK(image>=0 && !fstat(image,&c) && c.st_ino==a.st_ino && c.st_size==5);
  CHECK(!fstatat(AT_FDCWD,"data/image",&c,AT_SYMLINK_NOFOLLOW) && c.st_ino==a.st_ino);
  CHECK(fstatat(image,"child",&c,0)==-1 && errno==ENOTDIR);
  CHECK(!close(image));
  int fd=open("data/work",O_RDWR|O_CREAT|O_EXCL,0600);
  CHECK(fd>=0 && !fstat(fd,&a) && S_ISREG(a.st_mode) && a.st_size==0 && (a.st_mode&0777)==0600);
  int copy=dup(fd); CHECK(copy>=0);
  CHECK(write(fd,"abc",3)==3 && !fstat(copy,&b) && b.st_size==3 && b.st_ino==a.st_ino);
  CHECK(lseek(copy,4096,SEEK_SET)==4096 && write(fd,"X",1)==1);
  CHECK(!stat("data/work",&b) && b.st_size==4097 && b.st_ino==a.st_ino);
  CHECK(!close(fd) && !fstat(copy,&c) && c.st_ino==a.st_ino && c.st_size==4097);
  CHECK(c.st_atim.tv_sec>0 && c.st_mtim.tv_sec>0 && c.st_ctim.tv_sec>0);
  CHECK(c.st_atim.tv_nsec>=0 && c.st_atim.tv_nsec<1000000000);
  CHECK(c.st_mtim.tv_nsec>=0 && c.st_mtim.tv_nsec<1000000000);
  CHECK(c.st_ctim.tv_nsec>=0 && c.st_ctim.tv_nsec<1000000000);
  int trunc=open("data/work",O_WRONLY|O_TRUNC);
  CHECK(trunc>=0 && !fstat(copy,&b) && b.st_size==0 && b.st_ino==a.st_ino);
  CHECK(!close(trunc) && !close(copy));
  memset(&a,0xa5,sizeof(a)); b=a;
  CHECK(stat("data/missing",&a)==-1 && errno==ENOENT && !memcmp(&a,&b,sizeof(a)));
  CHECK(stat("data/image/child",&a)==-1 && errno==ENOTDIR);
  CHECK(fstat(-1,&a)==-1 && errno==EBADF);
  errno=123; CHECK(!stat("data/image",&a) && errno==123);
  printf("metadata identity, types, modes, writes, dup, truncate, errors: PASS\n");
  return 0;
}
#ifdef BLINK_MANAGED_METADATA
int MetadataPrivateErrors(void) {
  struct stat a;
  CHECK(stat("/etc/passwd",&a)==-1 && errno==ENOENT);
  CHECK(stat("../escape",&a)==-1 && errno==EACCES);
  CHECK(fstat(1,&a)==-1 && errno==ENOTSUP);
  CHECK(stat(0,&a)==-1 && errno==EFAULT);
  CHECK(stat("data/image",0)==-1 && errno==EFAULT);
  CHECK(fstatat(AT_FDCWD,"data/image",&a,4096)==-1 && errno==ENOTSUP);
  CHECK(!fstatat(-77,"/data/image",&a,0));
  CHECK(fstatat(-77,"data/image",&a,0)==-1 && errno==EBADF);
  CHECK(stat("",&a)==-1 && errno==ENOENT);
  CHECK(stat("data/image/",&a)==-1 && errno==ENOTDIR);
  CHECK(stat("data/\xff",&a)==-1 && errno==EINVAL);
  char longpath[4097]; memset(longpath,'x',4096); longpath[4096]=0;
  CHECK(stat(longpath,&a)==-1 && errno==36); // Measured ENAMETOOLONG below.
  CHECK(!stat("data/image",&a) && a.st_dev==1 && !a.st_uid && !a.st_gid && !a.st_rdev);
  CHECK(a.st_blocks==1 && a.st_blksize==4096);
  for (int i=0;i<3;++i) CHECK(!a.__reserved[i]);
  CHECK(open("data/image",O_WRONLY)==-1 && errno==EROFS);
  return 0;
}
#else
int main(void) { return MetadataProbe(); }
#endif
