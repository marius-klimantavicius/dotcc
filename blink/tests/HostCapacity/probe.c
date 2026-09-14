#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <string.h>
#include <sys/statvfs.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_CAPACITY
#include "host-errors.h"
#include "host-capacity.h"
#include "host-io.h"
#endif
#define CHECK(x) do { if(!(x)){printf("capacity failure %d\n",__LINE__);return __LINE__;} } while(0)
int CapacityProbe(void) {
  struct statvfs a,b,c;
  CHECK(!statvfs("data/image",&a));
  CHECK(a.f_bsize>0 && a.f_frsize>0 && a.f_bfree<=a.f_blocks && a.f_bavail<=a.f_bfree);
  CHECK(a.f_ffree<=a.f_files && a.f_favail<=a.f_ffree && a.f_namemax>0);
  int fd=open("data/image",O_RDONLY); CHECK(fd>=0);
  CHECK(!fstatvfs(fd,&b) && a.f_fsid==b.f_fsid && a.f_blocks==b.f_blocks && a.f_frsize==b.f_frsize);
  int copy=dup(fd);CHECK(copy>=0 && !close(fd) && !fstatvfs(copy,&c) && c.f_fsid==b.f_fsid);
  CHECK(!close(copy));
  fd=open("data",O_RDONLY|O_DIRECTORY);CHECK(fd>=0 && !fstatvfs(fd,&b) && b.f_fsid==a.f_fsid && !close(fd));
  CHECK(statvfs("data/missing",&b)==-1 && errno==ENOENT);
  CHECK(statvfs("data/image/child",&b)==-1 && errno==ENOTDIR);
  CHECK(fstatvfs(-1,&b)==-1 && errno==EBADF);
  puts("private filesystem capacity path, descriptor and error invariants: PASS");return 0;
}
#ifdef BLINK_MANAGED_CAPACITY
int CapacityExact(unsigned long total,unsigned long freebytes,unsigned long nodes) {
  struct statvfs a;
  errno=123;CHECK(!statvfs("/",&a) && errno==123);
  CHECK(a.f_bsize==4096 && a.f_frsize==1 && a.f_blocks==total && a.f_bfree==freebytes && a.f_bavail==freebytes);
  CHECK(a.f_files==1024 && a.f_ffree==1024-nodes && a.f_favail==a.f_ffree);
  CHECK(a.f_fsid==1 && a.f_flag==(ST_NOSUID|ST_NODEV) && a.f_namemax==4095);
  for(int i=0;i<6;++i)CHECK(!a.__reserved[i]);return 0;
}
int CapacityPrivate(void) {
  struct statvfs a,b;memset(&a,0xa5,sizeof(a));b=a;
  CHECK(statvfs("missing",&a)==-1 && errno==ENOENT && !memcmp(&a,&b,sizeof(a)));
  CHECK(fstatvfs(-1,&a)==-1 && errno==EBADF && !memcmp(&a,&b,sizeof(a)));
  CHECK(fstatvfs(1,&a)==-1 && errno==ENOTSUP && !memcmp(&a,&b,sizeof(a)));
  CHECK(statvfs(0,&a)==-1 && errno==EFAULT && !memcmp(&a,&b,sizeof(a)));
  CHECK(statvfs("/",0)==-1 && errno==EFAULT);
  CHECK(fstatvfs(1,0)==-1 && errno==EFAULT);
  CHECK(statvfs("../escape",&a)==-1 && errno==EACCES && !memcmp(&a,&b,sizeof(a)));
  CHECK(statvfs("/etc/passwd",&a)==-1 && errno==ENOENT);
  CHECK(statvfs("",&a)==-1 && errno==ENOENT);
  CHECK(statvfs("\xff",&a)==-1 && errno==EINVAL);
  char path[4097];memset(path,'x',4096);path[4096]=0;
  CHECK(statvfs(path,&a)==-1 && errno==ENAMETOOLONG && !memcmp(&a,&b,sizeof(a)));
  return 0;
}
int CapacityUnavailable(int error) {
  struct statvfs a,b;memset(&a,0xa5,sizeof(a));b=a;
  CHECK(statvfs("/",&a)==-1 && errno==error && !memcmp(&a,&b,sizeof(a)));
  CHECK(fstatvfs(0,&a)==-1 && errno==error && !memcmp(&a,&b,sizeof(a)));return 0;
}
int CapacityFdUnsupported(int fd) {struct statvfs a;CHECK(fstatvfs(fd,&a)==-1 && errno==ENOTSUP);return 0;}
int CapacityRelative(void) {struct statvfs a;CHECK(!statvfs("image",&a));return 0;}
#else
int main(void){return CapacityProbe();}
#endif
