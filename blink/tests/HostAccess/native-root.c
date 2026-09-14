#define _GNU_SOURCE 1
#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <stdlib.h>
#include <sys/stat.h>
#include <unistd.h>
#define CHECK(x) do { if(!(x)){printf("root access failure %d errno%d\n",__LINE__,errno);return __LINE__;} } while(0)
int main(void) {
  char path[]="/tmp/blink-access-root-XXXXXX";
  int fd=mkstemp(path);CHECK(fd>=0);CHECK(getuid()==0 && geteuid()==0);
  CHECK(!fchmod(fd,0000));CHECK(!access(path,R_OK|W_OK));
  CHECK(access(path,X_OK)==-1 && errno==EACCES);
  CHECK(!fchmod(fd,0100));CHECK(!access(path,X_OK));
  CHECK(!fchmod(fd,0010));CHECK(!access(path,X_OK));
  CHECK(!fchmod(fd,0001));CHECK(!access(path,X_OK));
  CHECK(!fchmod(fd,0444));CHECK(!access(path,W_OK));
  CHECK(access(path,X_OK)==-1 && errno==EACCES);
  CHECK(!close(fd) && !unlink(path));
  puts("native UID0 read/write bypass and any-bit executable requirement: PASS");return 0;
}
