#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <string.h>
#include <dirent.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_DIRECTORIES
#include "host-errors.h"
#include "host-io.h"
#include "host-directories.h"
#endif
#define CHECK(x) do {if(!(x)){printf("directory failure %d\n",__LINE__);return __LINE__;}}while(0)
/* Same record-size/strlen/aligned-copy shape as upstream Getdents, bounded by
 * a full record each iteration, with guards and independently decoded names. */
static int CopyEntries(DIR *stream) {
  unsigned char buffer[4098];memset(buffer,0xa5,sizeof(buffer));
  int count=0,seen=0;size_t used=0;struct dirent *entry;
  while(used+280<=4096) {
    errno=0;entry=readdir(stream);if(!entry){CHECK(!errno);break;}
    size_t length=strlen(entry->d_name);CHECK(length<256 && entry->d_ino);
    size_t size=(19+length+1+7)&~7;
    unsigned char record[280];memset(record,0,sizeof(record));
    memcpy(record,&entry->d_ino,8);record[16]=(unsigned char)size;record[17]=(unsigned char)(size>>8);
    record[18]=entry->d_type;strcpy((char*)record+19,entry->d_name);
    memcpy(buffer+1+used,record,size);used+=size;++count;
  }
  CHECK(buffer[0]==0xa5 && buffer[4097]==0xa5);
  for(size_t position=0;position<used;) {
    unsigned char *record=buffer+1+position;size_t size=record[16]|((size_t)record[17]<<8);
    CHECK(size>=24 && !(size&7) && position+size<=used);
    char *name=(char*)record+19;
    if(!strcmp(name,"."))seen|=1;else if(!strcmp(name,".."))seen|=2;
    else if(!strcmp(name,"image"))seen|=4;else if(!strcmp(name,"sub"))seen|=8;
    else CHECK(0);
    position+=size;
  }
  CHECK(count==4 && seen==15);return 0;
}
int DirectoryProbe(void) {
  int fd=open("data",O_RDONLY|O_DIRECTORY);CHECK(fd>=0);
  int copy=dup(fd);CHECK(copy>=0);
  DIR *stream=fdopendir(fd);CHECK(stream && dirfd(stream)==fd);
  struct dirent *first=readdir(stream);CHECK(first);
  long cookie=telldir(stream);CHECK(cookie>=0);
  struct dirent *second=readdir(stream);CHECK(second);char expected[256];strcpy(expected,second->d_name);
  seekdir(stream,cookie);second=readdir(stream);CHECK(second && !strcmp(second->d_name,expected));
  rewinddir(stream);CHECK(!CopyEntries(stream));
  errno=123;CHECK(!readdir(stream) && errno==123);
  CHECK(!closedir(stream));CHECK(close(fd)==-1 && errno==EBADF);
  stream=fdopendir(copy);CHECK(stream);rewinddir(stream);CHECK(!CopyEntries(stream) && !closedir(stream));
  fd=open("data/image",O_RDONLY);CHECK(fd>=0);CHECK(!fdopendir(fd) && errno==ENOTDIR);
  char byte;CHECK(read(fd,&byte,1)==1 && byte=='a' && !close(fd));
  stream=opendir("data");CHECK(stream && !CopyEntries(stream) && !closedir(stream));
  CHECK(!opendir("data/missing") && errno==ENOENT);
  puts("directory snapshots, native getdents-style records, ownership and cookies: PASS");return 0;
}
#ifdef BLINK_MANAGED_DIRECTORIES
int DirectoryPrivate(void) {
  CHECK(!fdopendir(-1) && errno==EBADF);CHECK(!opendir(0) && errno==EFAULT);
  CHECK(!opendir("../escape") && errno==EACCES);CHECK(!opendir("\xff") && errno==EINVAL);
  CHECK(!readdir((DIR*)1) && errno==EBADF && closedir((DIR*)1)==-1 && errno==EBADF);
  DIR *stream=opendir("data");CHECK(stream);struct dirent *entry=readdir(stream);
  CHECK(entry && !strcmp(entry->d_name,".") && entry->d_type==4);
  CHECK(readdir(stream) && !strcmp(readdir(stream)->d_name,"image"));
  long before=telldir(stream);errno=0;seekdir(stream,-1);CHECK(errno==EINVAL && telldir(stream)==before);
  CHECK(!closedir(stream) && !readdir(stream) && errno==EBADF);
  CHECK(closedir(stream)==-1 && errno==EBADF);
  return 0;
}
int DirectoryUnavailable(int error){CHECK(!opendir("/") && errno==error);CHECK(!readdir((DIR*)1) && errno==error);return 0;}
int DirectoryLong(void){int fd=open("/long",O_RDONLY|O_DIRECTORY);CHECK(fd>=0);CHECK(!fdopendir(fd) && errno==ENAMETOOLONG);CHECK(!close(fd));return 0;}
static _Thread_local DIR *worker_stream;
static _Thread_local struct dirent *worker_entry;
int DirectoryWorkerSetup(void){worker_stream=opendir("/");CHECK(worker_stream);worker_entry=readdir(worker_stream);CHECK(worker_entry && !strcmp(worker_entry->d_name,"."));return 0;}
int DirectoryWorkerFinish(int which){
  CHECK(worker_entry && !strcmp(worker_entry->d_name,"."));
  CHECK(readdir(worker_stream) && !strcmp(readdir(worker_stream)->d_name,which==1?"first":"second"));
  CHECK(!closedir(worker_stream));worker_stream=0;worker_entry=0;return 0;
}
#else
int main(void){return DirectoryProbe();}
#endif
