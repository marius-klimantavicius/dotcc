#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>
#include <string.h>
#ifdef BLINK_MANAGED_ACCESS
#include "host-errors.h"
#include "host-io.h"
#include "host-paths.h"
#include "host-access.h"
#define ROOT ""
#endif
#define CHECK(x) do { if(!(x)){printf("access failure %d\n",__LINE__);return __LINE__;} } while(0)
int AccessProbe(void) {
  int directory,file,duplicate;
  CHECK(!chdir(ROOT "/a"));
  CHECK(!access("image",F_OK) && !access("image",R_OK));
  CHECK(access("image",X_OK)==-1 && errno==EACCES);
  CHECK(!access("executable",X_OK|R_OK));
  CHECK(!access(".",R_OK|W_OK|X_OK));
  CHECK(access("missing",F_OK)==-1 && errno==ENOENT);
  CHECK(access("image/child",F_OK)==-1 && errno==ENOTDIR);
  CHECK(access("",F_OK)==-1 && errno==ENOENT);
  CHECK(access("image",8)==-1 && errno==EINVAL);
  CHECK(access("image",-1)==-1 && errno==EINVAL);
  directory=open(ROOT "/a",O_RDONLY|O_DIRECTORY);CHECK(directory>=0);
  file=open(ROOT "/a/image",O_RDONLY);CHECK(file>=0);
  CHECK(!chdir(ROOT "/other"));
  CHECK(!faccessat(directory,"image",R_OK,0));
  CHECK(!faccessat(directory,"executable",X_OK,AT_EACCESS|AT_SYMLINK_NOFOLLOW));
  CHECK(!faccessat(directory,"executable",X_OK,AT_EACCESS));
  CHECK(!faccessat(directory,"executable",X_OK,AT_SYMLINK_NOFOLLOW));
  CHECK(faccessat(file,"image",F_OK,0)==-1 && errno==ENOTDIR);
  CHECK(faccessat(-1,"image",F_OK,0)==-1 && errno==EBADF);
  CHECK(!faccessat(-1,ROOT "/a/image",F_OK,0));
  CHECK(faccessat(directory,"",F_OK,0)==-1 && errno==ENOENT);
  CHECK(faccessat(AT_FDCWD,"image",X_OK,0)==-1 && errno==EACCES);
  duplicate=dup(directory);CHECK(duplicate>=0 && !close(directory));
  CHECK(faccessat(directory,"image",F_OK,0)==-1 && errno==EBADF);
  CHECK(!faccessat(duplicate,"image",R_OK,0));CHECK(!close(duplicate));CHECK(!close(file));
  file=open("created",O_RDWR|O_CREAT|O_EXCL,0600);CHECK(file>=0 && !close(file));
  CHECK(!access("created",R_OK|W_OK));CHECK(access("created",X_OK)==-1 && errno==EACCES);
  errno=123;CHECK(!access("created",F_OK) && errno==123);
  puts("private access modes, canonical paths and descriptor-relative checks: PASS");
  return 0;
}
#ifdef BLINK_MANAGED_ACCESS
int AccessPrivate(void) {
  char bad[2]={(char)0xc0,0},longpath[4097];
  CHECK(access("/a/image",W_OK)==-1 && errno==EROFS);
  CHECK(access("/a/executable",W_OK|X_OK)==-1 && errno==EROFS);
  CHECK(!access("/a",W_OK|X_OK));
  CHECK(faccessat(AT_FDCWD,"/a/image",R_OK,4096)==-1 && errno==ENOTSUP);
  CHECK(access(0,F_OK)==-1 && errno==EFAULT);
  CHECK(access(bad,F_OK)==-1 && errno==EINVAL);
  memset(longpath,'x',4096);longpath[4096]=0;
  CHECK(access(longpath,F_OK)==-1 && errno==ENAMETOOLONG);
  CHECK(access("/../a/image",F_OK)==-1 && errno==EACCES);
  return 0;
}
int AccessWorker(int executable) {
  CHECK(!access("/same",R_OK));
  if(executable) CHECK(!access("/same",X_OK));
  else CHECK(access("/same",X_OK)==-1 && errno==EACCES);
  return 0;
}
int AccessUnavailable(int error) {
  CHECK(access("/",F_OK)==-1 && errno==error);
  CHECK(faccessat(AT_FDCWD,"/",F_OK,0)==-1 && errno==error);
  return 0;
}
#else
int main(void){return AccessProbe();}
#endif
