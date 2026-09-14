#define _GNU_SOURCE 1
#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <string.h>
#include <sys/stat.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_CONTROL
#include "host-io.h"
/* Exercise the same external C definitions used by actual core TUs. */
#undef open
#define open blink_host_open
#define IMAGE_ABSOLUTE "/data/image"
#else
#define IMAGE_ABSOLUTE BLINK_TEST_ROOT "/data/image"
#endif
#define CHECK(x) do { if (!(x)) { printf("failure %d\n",__LINE__); return __LINE__; } } while(0)
int ControlProbe(void) {
  printf("control constants %d %d %d %d %d %d %d %d\n",F_DUPFD,F_GETFD,F_SETFD,F_GETFL,F_SETFL,F_DUPFD_CLOEXEC,O_CLOEXEC,O_NDELAY);
  int dir=open("data",O_RDONLY|O_DIRECTORY|O_CLOEXEC);
  CHECK(dir>=0 && fcntl(dir,F_GETFD)==FD_CLOEXEC);
  struct stat st; CHECK(!fstat(dir,&st) && S_ISDIR(st.st_mode));
  int dircopy=dup(dir);CHECK(dircopy>=0 && !fcntl(dircopy,F_GETFD));
  CHECK(!close(dir));
  int mode=0600;
  int fd=openat(dircopy,"flags",O_RDWR|O_CREAT|O_EXCL|O_CLOEXEC,mode++);
  CHECK(fd>=0 && mode==0601 && fcntl(fd,F_GETFD)==FD_CLOEXEC);
  CHECK((fcntl(fd,F_GETFL)&(O_ACCMODE|O_APPEND))==O_RDWR);
  int copy=fcntl(fd,F_DUPFD,10);
  CHECK(copy>=10 && !fcntl(copy,F_GETFD));
  int clo=fcntl(fd,F_DUPFD_CLOEXEC,20);
  CHECK(clo>=20 && fcntl(clo,F_GETFD)==FD_CLOEXEC);
  CHECK(!fcntl(copy,F_SETFD,FD_CLOEXEC) && fcntl(copy,F_GETFD)==FD_CLOEXEC);
  CHECK(!fcntl(fd,F_SETFD,0) && !fcntl(fd,F_GETFD) && fcntl(copy,F_GETFD)==FD_CLOEXEC);
  CHECK(write(fd,"abc",3)==3 && lseek(copy,0,SEEK_SET)==0);
  CHECK(!fcntl(copy,F_SETFL,fcntl(copy,F_GETFL)|O_APPEND));
  CHECK((fcntl(fd,F_GETFL)&O_APPEND) && write(fd,"D",1)==1);
  CHECK(!fstat(clo,&st) && st.st_size==4);
  CHECK(!fcntl(clo,F_SETFL,O_RDONLY)); // Access mode is ignored by F_SETFL.
  CHECK((fcntl(fd,F_GETFL)&(O_APPEND|O_ACCMODE))==O_RDWR);
  CHECK(lseek(fd,0,SEEK_SET)==0 && write(copy,"Z",1)==1);
  char data[8]={0};CHECK(lseek(clo,0,SEEK_SET)==0 && read(fd,data,8)==4 && !memcmp(data,"ZbcD",4));
  int side=0;
  CHECK(fcntl(fd,F_GETFL,side++)>=0 && side==1);
  CHECK(!fcntl(fd,F_SETFD,0,side++) && side==2);
  CHECK(fcntl(-1,F_GETFD)==-1 && errno==EBADF);
  CHECK(fcntl(fd,F_DUPFD,-1)==-1 && errno==EINVAL);
  CHECK(openat(fd,"child",O_RDONLY)==-1 && errno==ENOTDIR);
  CHECK(openat(-99,"relative",O_RDONLY)==-1 && errno==EBADF);
  CHECK(openat(-99,"",O_RDONLY)==-1 && errno==ENOENT);
  CHECK(openat(dircopy,"image",O_RDONLY|O_DIRECTORY)==-1 && errno==ENOTDIR);
  CHECK(openat(dircopy,"missing",O_RDONLY)==-1 && errno==ENOENT);
  CHECK(!fstatat(dircopy,"image",&st,AT_SYMLINK_NOFOLLOW) && st.st_size==5);
  int absolute=openat(-99,IMAGE_ABSOLUTE,O_RDONLY);
  CHECK(absolute>=0 && !fstatat(-99,IMAGE_ABSOLUTE,&st,0) && st.st_size==5 && !close(absolute));
  int image=openat(dircopy,"image",O_RDONLY|O_NOFOLLOW|O_NOCTTY);
  CHECK(image>=0 && (fcntl(image,F_GETFL)&O_NOFOLLOW));
  CHECK(openat(dircopy,"image/child",O_RDONLY)==-1 && errno==ENOTDIR);
  CHECK(!close(image) && !close(copy) && !close(clo) && !close(fd) && !close(dircopy));
  printf("openat, shared status, per-fd flags, dup, append and variadic evaluation: PASS\n");
  return 0;
}
#ifdef BLINK_MANAGED_CONTROL
int ControlPrivateProbe(void) {
  int fd=open("data/flags",O_RDWR);CHECK(fd>=0);
  int before=fcntl(fd,F_GETFL);
  CHECK(fcntl(fd,F_SETFL,before|O_NONBLOCK)==-1 && errno==ENOTSUP && fcntl(fd,F_GETFL)==before);
  CHECK(open("data/unsupported",O_WRONLY|O_CREAT|O_NONBLOCK,0600)==-1 && errno==ENOTSUP);
  struct stat st;CHECK(stat("data/unsupported",&st)==-1 && errno==ENOENT);
  CHECK(fcntl(fd,F_SETFD,2)==-1 && errno==EINVAL && !fcntl(fd,F_GETFD));
  CHECK(fcntl(fd,123456)==-1 && errno==ENOTSUP);
  struct flock lock={0};int side=0;
  CHECK(fcntl(fd,F_GETLK,(++side,&lock))==-1 && errno==ENOTSUP && side==1);
  int absolute=openat(-99,"/data/image",O_RDONLY);CHECK(absolute>=0 && !close(absolute));
  CHECK(openat(AT_FDCWD,"/etc/passwd",O_RDONLY)==-1 && errno==ENOENT);
  int dir=open("data",O_RDONLY);CHECK(dir>=0);
  char byte;CHECK(read(dir,&byte,1)==-1 && errno==EISDIR);
  CHECK(openat(dir,"../../escape",O_RDONLY)==-1 && errno==EACCES);
  CHECK(!close(dir) && !close(fd));
  return 0;
}
#else
int main(void) { return ControlProbe(); }
#endif
