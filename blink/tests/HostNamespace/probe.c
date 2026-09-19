#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <string.h>
#include <sys/stat.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_NAMESPACE
#include "host-errors.h"
#include "host-io.h"
#include "host-namespace.h"
#include "host-paths.h"
#endif
#define CHECK(x) do { if (!(x)) {printf("namespace failure %d errno %d\n",__LINE__,errno);return __LINE__;} } while(0)
int NamespaceProbe(void) {
 struct stat a,b;char bytes[4]={0};char cwd[128];
 int parent=open("data",O_RDONLY|O_DIRECTORY);CHECK(parent>=0);
 CHECK(!mkdirat(parent,"a/",0700));
 CHECK(mkdirat(parent,"a",0700)==-1 && errno==EEXIST);
 int directory=open("data/a",O_RDONLY|O_DIRECTORY);CHECK(directory>=0);
 int child=open("data/a/child",O_CREAT|O_RDWR|O_EXCL,0600);CHECK(child>=0 && write(child,"old",3)==3);
 CHECK(!fstat(directory,&a));
 CHECK(unlinkat(parent,"a",AT_REMOVEDIR)==-1 && errno==ENOTEMPTY);
 CHECK(!renameat(parent,"a",parent,"moved/") && !fstat(directory,&b) && a.st_ino==b.st_ino);
 CHECK(stat("data/a",&a)==-1 && errno==ENOENT);
 CHECK(!chdir("data/moved") && getcwd(cwd,sizeof(cwd)) && strstr(cwd,"data/moved"));
 CHECK(!rename("../moved","../renamed"));
 CHECK(getcwd(cwd,sizeof(cwd)) && strstr(cwd,"data/renamed"));
 CHECK(!chdir("../.."));
 CHECK(rename("data/renamed","data/renamed/deeper")==-1 && errno==EINVAL);
 CHECK(!mkdir("data/empty",0777));CHECK(!stat("data/empty",&a) && (a.st_mode&0777)==0755);
 CHECK(rename("data/renamed/child","data/empty")==-1 && errno==EISDIR);
 CHECK(rename("data/empty","data/renamed/child")==-1 && errno==ENOTDIR);
 int replacement=open("data/replacement",O_CREAT|O_RDWR|O_EXCL,0600);CHECK(replacement>=0 && write(replacement,"new",3)==3);
 CHECK(!fstat(child,&a));
 CHECK(!rename("data/replacement","data/renamed/child"));
 CHECK(!fstat(child,&b) && b.st_ino==a.st_ino && b.st_nlink==0);
 CHECK(lseek(child,0,SEEK_SET)==0 && read(child,bytes,3)==3 && !memcmp(bytes,"old",3));
 CHECK(!stat("data/renamed/child",&b) && b.st_ino!=a.st_ino);
 CHECK(!unlinkat(directory,"child",0));
 CHECK(!fstat(replacement,&b) && b.st_nlink==0 && b.st_size==3);
 CHECK(lseek(replacement,0,SEEK_SET)==0 && read(replacement,bytes,3)==3 && !memcmp(bytes,"new",3));
 CHECK(!close(child) && !close(replacement));
 CHECK(!rmdir("data/renamed") && !fstat(directory,&b) && S_ISDIR(b.st_mode) && b.st_nlink==0);
 CHECK(!close(directory));
 CHECK(!rmdir("data/empty"));
 CHECK(!close(parent));
 puts("namespace common pass");return 0;
}
#ifdef BLINK_MANAGED_NAMESPACE
int NamespacePrivate(void) {
 struct stat before,after;CHECK(!stat("data/image",&before));
 CHECK(unlink("data/image")==-1 && errno==EROFS);
 CHECK(rename("data/image","data/other")==-1 && errno==EROFS);
 CHECK(!stat("data/image",&after) && before.st_ino==after.st_ino);
 CHECK(!mkdir("data/work",0700));
 CHECK(mkdir("data/special",04700)==-1 && errno==ENOTSUP);
 CHECK(unlinkat(AT_FDCWD,"data/work",123)==-1 && errno==ENOTSUP);
 CHECK(!chdir("data/work"));
 CHECK(rmdir("/data/work")==-1 && errno==EBUSY);
 CHECK(!mkdir("/data/replacer",0700));
 CHECK(rename("/data/replacer","/data/work")==-1 && errno==EBUSY);
 CHECK(!chdir("/"));
 CHECK(!rmdir("data/work") && !rmdir("data/replacer"));
 CHECK(mkdirat(-999,"relative",0700)==-1 && errno==EBADF);
 CHECK(!mkdirat(-999,"/data/absolute",0700));CHECK(!rmdir("/data/absolute"));
 CHECK(mkdir("../../escape",0700)==-1 && errno==EACCES);
 return 0;
}
#else
int main(void){umask(0022);return NamespaceProbe();}
#endif
