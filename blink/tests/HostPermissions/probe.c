#define _GNU_SOURCE
#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/stat.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_PERMISSIONS
#include "host-io.h"
#include "host-permissions.h"
#include "host-identity.h"
#include "host-access.h"
#include "host-paths.h"
#include "host-namespace.h"
void PermissionsGc(void);
#else
static void PermissionsGc(void) {}
#endif
#define CHECK(x) do{if(!(x))return __LINE__;}while(0)
int Permissions(unsigned int mask) {
 struct stat before,after;
 CHECK(umask(mask)==0022);
 CHECK(!mkdir("work",0777));
 CHECK(!stat("work",&after) && (after.st_mode&0777)==(0777&~mask));
 int side=1;int directory=open("work",O_RDONLY|O_DIRECTORY,side++);CHECK(directory>=0 && side==2);
 unsigned int mode=0666;
 int fd=openat(directory,"file",O_RDWR|O_CREAT|O_EXCL,mode++);CHECK(fd>=0 && mode==0667);
 CHECK(!fstat(fd,&before) && (before.st_mode&0777)==(0666&~mask));
 CHECK(write(fd,"data",4)==4);
 int duplicate=dup(fd);CHECK(duplicate>=0);
 CHECK(!fchmod(duplicate,0111));
 CHECK(!fstat(fd,&after) && after.st_ino==before.st_ino && (after.st_mode&0777)==0111);
 CHECK(!access("work/file",X_OK));
 CHECK(!fchmodat(directory,"file",0,0));
 CHECK(access("work/file",X_OK)==-1 && errno==EACCES);
 CHECK(write(fd,"!",1)==1); /* Existing descriptor rights survive chmod. */
 CHECK(!fchown(fd,getuid(),getgid()) && !fchown(duplicate,(uid_t)-1,(gid_t)-1));
 CHECK(!fchownat(directory,"file",(uid_t)-1,(gid_t)-1,AT_SYMLINK_NOFOLLOW));
 CHECK(!fstat(fd,&after) && after.st_uid==getuid() && after.st_gid==getgid() && after.st_size==5);
 char path[4096];CHECK(getcwd(path,sizeof(path))!=0);strcat(path,"/work/file");
 CHECK(!fchmodat(-123,path,0700,0));
 CHECK(!fchmodat(directory,"file",0640,AT_SYMLINK_NOFOLLOW));
 CHECK(!fstat(fd,&before) && (before.st_mode&0777)==0640);
 CHECK(fchmodat(fd,"child",0600,0)==-1 && errno==ENOTDIR);
 CHECK(fchownat(directory,"missing",(uid_t)-1,(gid_t)-1,0)==-1 && errno==ENOENT);
 CHECK(fchown(-1,(uid_t)-1,(gid_t)-1)==-1 && errno==EBADF);
 PermissionsGc();
 CHECK(!fstat(fd,&after) && after.st_ino==before.st_ino && (after.st_mode&0777)==0640);
 CHECK(umask(0)==mask);
 int opened=open("work/open-mode",O_WRONLY|O_CREAT|O_EXCL,0624);CHECK(opened>=0);
 CHECK(!fstat(opened,&after) && (after.st_mode&0777)==0624);CHECK(!close(opened));
 CHECK(umask((mode_t)-1)==0);CHECK(umask(mask)==0777);
#ifdef BLINK_MANAGED_PERMISSIONS
 CHECK(fchmod(fd,04000)==-1 && errno==EOPNOTSUPP);
 CHECK(fchmodat(directory,"file",0600,0x40000000)==-1 && errno==EOPNOTSUPP);
 CHECK(fchown(fd,123,0)==-1 && errno==EPERM);
 CHECK(fchownat(directory,"file",0,123,0)==-1 && errno==EPERM);
 CHECK(!fstat(fd,&after) && !memcmp(&before,&after,sizeof(before)));
 CHECK(open("work/unsupported",O_WRONLY|O_CREAT|O_EXCL,04777)==-1 && errno==EOPNOTSUPP);
 CHECK(access("work/unsupported",F_OK)==-1 && errno==ENOENT);
 CHECK(!fchownat(fd,"",(uid_t)-1,(gid_t)-1,4096));
 CHECK(!fchownat(-123,path,0,0,0));
 char invalid[]={ (char)0xff,0 };
 CHECK(fchmodat(directory,invalid,0600,0)==-1 && errno==EINVAL);
 int image=open("image",O_RDONLY);CHECK(image>=0);
 CHECK(!fstat(image,&before));
 CHECK(fchmod(image,0600)==-1 && errno==EROFS);
 CHECK(fchown(image,0,0)==-1 && errno==EROFS);
 CHECK(fchownat(AT_FDCWD,"image",(uid_t)-1,(gid_t)-1,0)==-1 && errno==EROFS);
 CHECK(!fstat(image,&after) && !memcmp(&before,&after,sizeof(before)));CHECK(!close(image));
#endif
 CHECK(!close(duplicate) && !close(fd) && !close(directory));
 return 0;
}
#ifdef BLINK_MANAGED_PERMISSIONS
int UnboundPermissions(int expected) {
 CHECK(fchmod(-1,0600)==-1 && errno==expected);
 CHECK(fchown(-1,0,0)==-1 && errno==expected);
 CHECK(umask(0)==(mode_t)-1 && errno==expected);
 return 0;
}
#else
int main(void){umask(0022);int result=Permissions(0027);if(result){fprintf(stderr,"line %d errno %d\n",result,errno);return 1;}puts("private permission/ownership/creation invariants: PASS");return 0;}
#endif
