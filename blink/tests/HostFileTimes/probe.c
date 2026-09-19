#include <errno.h>
#include <fcntl.h>
#include <stddef.h>
#include <stdio.h>
#include <stdint.h>
#include <sys/stat.h>
#include <time.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_TIMES
#include "host-io.h"
#include "host-file-times.h"
#endif
#define CHECK(x) do { if (!(x)) { printf("file-times failure %d errno %d\n",__LINE__,errno); return __LINE__; } } while(0)
#define SAME(a,b) ((a).tv_sec==(b).tv_sec && (a).tv_nsec==(b).tv_nsec)
int TimesLayout(void) {
 printf("timespec %zu %zu %zu %zu %ld %ld\n",sizeof(struct timespec),_Alignof(struct timespec),offsetof(struct timespec,tv_sec),offsetof(struct timespec,tv_nsec),(long)UTIME_NOW,(long)UTIME_OMIT);
 return 0;
}
int TimesProbe(void) {
 int fd=open("data/work",O_CREAT|O_RDWR|O_TRUNC,0600); CHECK(fd>=0);
 int copy=dup(fd); CHECK(copy>=0);
 int dir=open("data",O_RDONLY|O_DIRECTORY); CHECK(dir>=0);
 struct timespec t[2]={{-1,1},{123456789,999999999}};
 struct stat a,b;
 int (*setfd)(int,const struct timespec*)=futimens;
 CHECK(!setfd(copy,t) && !fstat(fd,&a));
 CHECK(SAME(a.st_atim,t[0]) && SAME(a.st_mtim,t[1]));
 CHECK(a.st_ctim.tv_sec>0 && a.st_ctim.tv_nsec>=0 && a.st_ctim.tv_nsec<1000000000);
 t[0].tv_sec=5;t[0].tv_nsec=2;t[1].tv_nsec=1000000000;
 CHECK(futimens(fd,t)==-1 && errno==EINVAL && !fstat(fd,&b));
 CHECK(SAME(a.st_atim,b.st_atim) && SAME(a.st_mtim,b.st_mtim) && SAME(a.st_ctim,b.st_ctim));
 t[0].tv_sec=INT64_MIN;t[0].tv_nsec=UTIME_OMIT;t[1].tv_sec=INT64_MAX;t[1].tv_nsec=UTIME_OMIT;
 errno=0;CHECK(!utimensat(-999,"does/not/exist",t,123456));
 CHECK(futimens(-999,t)==-1 && errno==EBADF);
 CHECK(!futimens(fd,t) && !fstat(fd,&b) && SAME(a.st_ctim,b.st_ctim));
 t[0].tv_nsec=UTIME_NOW;t[1].tv_sec=42;t[1].tv_nsec=37;
 CHECK(!utimensat(dir,"work",t,AT_SYMLINK_NOFOLLOW) && !fstat(fd,&b));
 CHECK(b.st_atim.tv_sec>0 && SAME(b.st_mtim,t[1]));
 t[0].tv_sec=123;t[0].tv_nsec=999;t[1].tv_nsec=UTIME_OMIT;
 CHECK(!utimensat(AT_FDCWD,"data/work",t,0) && !fstat(fd,&a));
 CHECK(SAME(a.st_atim,t[0]) && SAME(a.st_mtim,b.st_mtim));
 CHECK(!futimens(fd,0) && !fstat(fd,&a) && SAME(a.st_atim,a.st_mtim) && a.st_atim.tv_sec>0);
 CHECK(utimensat(fd,"child",0,0)==-1 && errno==ENOTDIR);
 CHECK(utimensat(-999,"relative",0,0)==-1 && errno==EBADF);
 CHECK(!close(fd) && !futimens(copy,0));
 CHECK(!close(copy) && !close(dir));
 puts("file-times common pass");return 0;
}
#ifdef BLINK_MANAGED_TIMES
int TimesPrivate(void) {
 struct timespec t[2]={{-62135596800LL,1},{253402300799LL,999999999}};
 struct stat a,b;
 CHECK(!utimensat(-999,"/data/work",t,0) && !stat("data/work",&a));
 CHECK(SAME(a.st_atim,t[0]) && SAME(a.st_mtim,t[1]));
 t[0].tv_sec=-62135596801LL;
 CHECK(utimensat(AT_FDCWD,"data/work",t,0)==-1 && errno==EINVAL);
 t[0].tv_sec=5;t[1].tv_sec=253402300800LL;
 CHECK(utimensat(AT_FDCWD,"data/work",t,0)==-1 && errno==EINVAL);
 CHECK(!stat("data/work",&b) && SAME(a.st_atim,b.st_atim) && SAME(a.st_mtim,b.st_mtim) && SAME(a.st_ctim,b.st_ctim));
 t[1].tv_sec=8;t[1].tv_nsec=-1;
 CHECK(utimensat(AT_FDCWD,"data/work",t,0)==-1 && errno==EINVAL);
 t[1].tv_nsec=0;
 CHECK(utimensat(AT_FDCWD,"data/work",t,4096)==-1 && errno==ENOTSUP);
 CHECK(utimensat(AT_FDCWD,0,t,0)==-1 && errno==EFAULT);
 CHECK(!stat("data/image",&a));
 CHECK(utimensat(AT_FDCWD,"data/image",t,0)==-1 && errno==EROFS);
 int fd=open("data/image",O_RDONLY);CHECK(fd>=0);
 CHECK(futimens(fd,0)==-1 && errno==EROFS);
 t[0].tv_nsec=t[1].tv_nsec=UTIME_OMIT;CHECK(!futimens(fd,t));
 CHECK(!fstat(fd,&b) && SAME(a.st_ctim,b.st_ctim));CHECK(!close(fd));
 return 0;
}
#else
int main(void){if(TimesLayout())return 1;return TimesProbe();}
#endif
