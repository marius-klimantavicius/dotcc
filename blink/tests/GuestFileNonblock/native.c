#define _GNU_SOURCE 1
#include <fcntl.h>
#include <dirent.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#define CHECK(x) do { if (!(x)) return __LINE__; } while (0)
int main(void) {
  int d = open(".", O_RDONLY|O_NONBLOCK|O_DIRECTORY|O_CLOEXEC);
  CHECK(d >= 0 && (fcntl(d,F_GETFL)&(O_NONBLOCK|O_DIRECTORY)) == (O_NONBLOCK|O_DIRECTORY));
  CHECK(fcntl(d,F_GETFD) == FD_CLOEXEC);
  int copy = dup(d);
  CHECK(copy >= 0 && fcntl(copy,F_GETFD) == 0);
  CHECK(!fcntl(copy,F_SETFL,O_DIRECTORY) && !(fcntl(d,F_GETFL)&O_NONBLOCK));
  CHECK(!fcntl(d,F_SETFL,O_DIRECTORY|O_NONBLOCK));
  DIR *directory = fdopendir(copy); CHECK(directory);
  int found = 0; struct dirent *e;
  while ((e = readdir(directory))) if (!strcmp(e->d_name,"seed")) found = 1;
  CHECK(found && !closedir(directory));
  int fd = openat(d,"seed",O_RDONLY|O_NONBLOCK); CHECK(fd>=0);
  char b[4]; CHECK(read(fd,b,4)==3 && !memcmp(b,"abc",3) && read(fd,b,4)==0 && !close(fd));
  fd = open("created",O_RDWR|O_CREAT|O_EXCL|O_NONBLOCK,0600); CHECK(fd>=0);
  copy=dup(fd); CHECK(copy>=0 && write(fd,"a",1)==1 && lseek(fd,0,SEEK_SET)==0);
  CHECK(!fcntl(copy,F_SETFL,O_RDWR|O_APPEND|O_NONBLOCK) && write(copy,"b",1)==1);
  CHECK((fcntl(fd,F_GETFL)&(O_ACCMODE|O_APPEND|O_NONBLOCK)) == (O_RDWR|O_APPEND|O_NONBLOCK));
  CHECK(!fcntl(copy,F_SETFL,0) && (fcntl(fd,F_GETFL)&(O_ACCMODE|O_APPEND|O_NONBLOCK))==O_RDWR);
  CHECK(lseek(fd,0,SEEK_SET)==0 && read(fd,b,4)==2 && !memcmp(b,"ab",2));
  CHECK(!close(copy) && !close(fd) && !close(d));
  puts("ordinary nonblock: directory enumeration, read/EOF, append and dup flags: PASS");
  return 0;
}
