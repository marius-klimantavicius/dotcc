#ifndef BLINK_NAMESPACE_POLICY
#define _POSIX_C_SOURCE 200809L
#endif
#include <fcntl.h>
#include <unistd.h>
#include <sys/stat.h>
#include <sys/socket.h>
#include <errno.h>
#include <stdio.h>
#ifdef BLINK_NAMESPACE_POLICY
#include "host-errors.h"
#include "host-profile-constants.h"
#include "host-io.h"
#include "host-namespace-policy.h"
#endif
#define CHECK(x) do{if(!(x)){printf("namespace policy line %d errno %d\n",__LINE__,errno);return __LINE__;}}while(0)
int NamespacePolicyCommon(void) {
 int fd=open("work",O_CREAT|O_RDWR|O_TRUNC,0600);CHECK(fd>=0);
 char output[3]={11,22,33};
 CHECK(readlinkat(AT_FDCWD,"work",output,3)==-1 && errno==EINVAL);
 CHECK(readlinkat(AT_FDCWD,"missing",output,3)==-1 && errno==ENOENT);
 CHECK(readlinkat(fd,"child",output,3)==-1 && errno==ENOTDIR);
 CHECK(readlinkat(-999,"relative",output,3)==-1 && errno==EBADF);
 CHECK(output[0]==11 && output[1]==22 && output[2]==33);
 int pair[2]={123,456};
 CHECK(socketpair(AF_INET,SOCK_STREAM,0,pair)==-1 && errno==EOPNOTSUPP);
 CHECK(socketpair(32767,SOCK_STREAM,0,pair)==-1 && errno==EAFNOSUPPORT);
#ifdef BLINK_NAMESPACE_POLICY
 CHECK(pair[0]==123 && pair[1]==456);
#endif
 CHECK(!close(fd));return 0;
}
#ifdef BLINK_NAMESPACE_POLICY
void NamespacePolicyGc(void);
int NamespacePolicyUnavailable(int error) {
 char output[2]={11,22};int pair[2]={33,44};
 CHECK(readlinkat(AT_FDCWD,"work",output,2)==-1 && errno==error);
 CHECK(mkfifoat(AT_FDCWD,"fifo",0600)==-1 && errno==error);
 CHECK(socketpair(AF_INET,SOCK_STREAM,0,pair)==-1 && errno==error);
 CHECK(output[0]==11 && output[1]==22 && pair[0]==33 && pair[1]==44);return 0;
}
int NamespacePolicyPrivate(void) {
 CHECK(linkat(AT_FDCWD,"work",AT_FDCWD,"alias",0)==-1 && errno==ENOTSUP);
 CHECK(linkat(AT_FDCWD,"missing",AT_FDCWD,"alias",0)==-1 && errno==ENOENT);
 CHECK(linkat(AT_FDCWD,"work",AT_FDCWD,"alias",4096)==-1 && errno==ENOTSUP);
 CHECK(linkat(AT_FDCWD,"work",AT_FDCWD,"work",0)==-1 && errno==EEXIST);
 CHECK(linkat(AT_FDCWD,"image",AT_FDCWD,"alias",0)==-1 && errno==EROFS);
 CHECK(linkat(AT_FDCWD,"/",AT_FDCWD,"alias",0)==-1 && errno==EPERM);
 CHECK(symlinkat("../outside",AT_FDCWD,"symbolic")==-1 && errno==ENOTSUP);
 CHECK(symlinkat("work",AT_FDCWD,"work")==-1 && errno==EEXIST);
 CHECK(symlinkat("work",-999,"symbolic")==-1 && errno==EBADF);
 CHECK(mkfifoat(AT_FDCWD,"fifo",0600)==-1 && errno==ENOTSUP);
 CHECK(mkfifoat(AT_FDCWD,"../outside",0600)==-1 && errno==EACCES);
 CHECK(mkfifoat(AT_FDCWD,"missing/child",0600)==-1 && errno==ENOENT);
 CHECK(mkfifoat(AT_FDCWD,"work",0600)==-1 && errno==EEXIST);
 CHECK(mkfifoat(AT_FDCWD,"work/child",0600)==-1 && errno==ENOTDIR);
 char invalid[]={ (char)255,0 };CHECK(mkfifoat(AT_FDCWD,invalid,0600)==-1 && errno==EINVAL);
 char value=55;CHECK(readlinkat(-999,"/work",&value,1)==-1 && errno==EINVAL && value==55);
 CHECK(readlinkat(AT_FDCWD,"work",&value,0)==-1 && errno==EINVAL && value==55);
 int pair[2]={71,72};CHECK(socketpair(1,SOCK_STREAM,0,pair)==-1 && errno==EAFNOSUPPORT && pair[0]==71 && pair[1]==72);
 NamespacePolicyGc();
 CHECK(open("alias",O_RDONLY)==-1 && errno==ENOENT);
 CHECK(open("symbolic",O_RDONLY)==-1 && errno==ENOENT);
 CHECK(open("fifo",O_RDONLY)==-1 && errno==ENOENT);
 int fd=open("work",O_RDONLY);CHECK(fd>=0 && !close(fd));
 return 0;
}
#else
int main(int argc,char**argv) {
 if(argc!=2 || chdir(argv[1]))return 2;
 if(NamespacePolicyCommon())return 1;
 unlink("work");puts("namespace and network profile invariants: PASS");return 0;
}
#endif
