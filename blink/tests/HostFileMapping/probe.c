#define _GNU_SOURCE 1
#include <errno.h>
#include <fcntl.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/mman.h>
#include <sys/stat.h>
#include <unistd.h>
#include "HostFileMapping.h"
#ifdef BLINK_MANAGED_FILEMAP
#include "host-io.h"
#endif
#if defined(BLINK_MANAGED_FILEMAP) || defined(BLINK_NATIVE_ADAPTER)
#define OWNED 1
#endif
#define FILE_LENGTH 131123
#define CHECK(x) do { if (!(x)) { printf("failure %d\n",__LINE__); return __LINE__; } } while(0)
#ifdef OWNED
static ssize_t TestRead(int fd, void *destination, size_t length, off_t offset) {
  return pread(fd,destination,length,offset);
}
static int TestLength(int fd, off_t *length) {
#ifdef BLINK_MANAGED_FILEMAP
  return blink_io_read_at_length(fd,length);
#else
  struct stat st;
  if(fstat(fd,&st))return -1;
  if((fcntl(fd,F_GETFL)&O_ACCMODE)==O_WRONLY){errno=EBADF;return -1;}
  if(!S_ISREG(st.st_mode)){errno=EISDIR;return -1;}
  *length=st.st_size;return 0;
#endif
}
static int EnableFiles(void) {
#ifdef BLINK_MANAGED_FILEMAP
  return BlinkHostMemoryEnablePrivateFiles();
#else
  return BlinkHostMemorySetFileReader(TestRead,TestLength);
#endif
}
static int fault_calls;
static ssize_t FaultRead(int fd, void *destination, size_t length, off_t offset) {
  if(++fault_calls==2){errno=EIO;return -1;}
  return TestRead(fd,destination,length,offset);
}
static ssize_t ShortRead(int fd, void *destination, size_t length, off_t offset) {
  if(length>16384)length=16384;
  return TestRead(fd,destination,length,offset);
}
static ssize_t EmptyRead(int fd, void *destination, size_t length, off_t offset) {
  (void)fd;(void)destination;(void)length;(void)offset;return 0;
}
static int PrivateChecks(void) {
  int fd=open("image",O_RDONLY);CHECK(fd>=0);
  CHECK(!BlinkHostMemoryMappings() && !BlinkHostMemoryBytes());
  CHECK(mmap(0,4096,3,MAP_PRIVATE,fd,1)==MAP_FAILED && errno==EINVAL);
  CHECK(mmap(0,4096,3,MAP_PRIVATE,fd,-4096)==MAP_FAILED && errno==EINVAL);
  CHECK(mmap(0,4096,PROT_READ,MAP_PRIVATE,fd,0)==MAP_FAILED && errno==ENOTSUP);
  CHECK(mmap(0,4096,3,MAP_SHARED,fd,0)==MAP_FAILED && errno==ENOTSUP);
  CHECK(mmap((void*)4096,4096,3,MAP_PRIVATE|MAP_FIXED,fd,0)==MAP_FAILED && errno==ENOTSUP);
  CHECK(mmap(0,8192,3,MAP_PRIVATE,fd,131072)==MAP_FAILED && errno==ENOTSUP);
  CHECK(mmap(0,4096,3,MAP_PRIVATE,fd,135168)==MAP_FAILED && errno==ENOTSUP);
  CHECK(mmap(0,4096,3,MAP_PRIVATE,-1,0)==MAP_FAILED && errno==EBADF);
  int empty=open("empty",O_RDONLY);CHECK(empty>=0);
  CHECK(mmap(0,1,3,MAP_PRIVATE,empty,0)==MAP_FAILED && errno==ENOTSUP && !close(empty));
  int writeonly=open("writeonly",O_WRONLY|O_CREAT,0600);CHECK(writeonly>=0 && write(writeonly,"x",1)==1);
  CHECK(mmap(0,1,3,MAP_PRIVATE,writeonly,0)==MAP_FAILED && errno==EBADF && !close(writeonly));
  CHECK(!BlinkHostMemoryBytes() && !BlinkHostMemoryMappings());
  fault_calls=0;CHECK(!BlinkHostMemorySetFileReader(FaultRead,TestLength));
  CHECK(mmap(0,FILE_LENGTH,3,MAP_PRIVATE,fd,0)==MAP_FAILED && errno==EIO && fault_calls==2);
  CHECK(!BlinkHostMemoryBytes() && !BlinkHostMemoryMappings());
  CHECK(!BlinkHostMemorySetFileReader(EmptyRead,TestLength));
  CHECK(mmap(0,4096,3,MAP_PRIVATE,fd,0)==MAP_FAILED && errno==EIO);
  CHECK(!BlinkHostMemoryBytes() && !BlinkHostMemoryMappings());
  CHECK(!BlinkHostMemorySetFileReader(ShortRead,TestLength));
  unsigned char *p=mmap(0,FILE_LENGTH,3,MAP_PRIVATE,fd,0);CHECK(p!=MAP_FAILED);
  CHECK(BlinkHostMemoryBytes()==135168+24 && BlinkHostMemoryMappings()==1);
  CHECK(BlinkHostMemoryContains(p,135168) && !BlinkHostMemoryContains(p+135167,2));
  for(int i=0;i<FILE_LENGTH;++i)CHECK(p[i]==(unsigned char)(i%251));
  CHECK(!munmap(p,FILE_LENGTH) && !BlinkHostMemoryEnd());
  CHECK(!BlinkHostMemoryBegin(4096+24) && !EnableFiles());
  CHECK(mmap(0,FILE_LENGTH,3,MAP_PRIVATE,fd,0)==MAP_FAILED && errno==ENOMEM);
  CHECK(!BlinkHostMemoryBytes() && !BlinkHostMemoryMappings() && !close(fd));
  CHECK(!BlinkHostMemoryEnd() && !BlinkHostMemoryBegin(1024*1024));
  // End clears callbacks; a later anonymous-only owner cannot inherit I/O.
  CHECK(mmap(0,4096,3,MAP_PRIVATE,0,0)==MAP_FAILED && errno==ENOTSUP);
  CHECK(!BlinkHostMemoryEnd());
  return 0;
}
#endif
int FileMappingProbe(void) {
#ifdef OWNED
  CHECK(!BlinkHostMemoryBegin(1024*1024) && !EnableFiles());
#endif
  int fd=open("image",O_RDONLY);CHECK(fd>=0);
  CHECK(lseek(fd,17,SEEK_SET)==17);
  char bytes[8];CHECK(pread(fd,bytes,8,100)==8 && lseek(fd,0,SEEK_CUR)==17);
  for(int i=0;i<8;++i)CHECK((unsigned char)bytes[i]==(unsigned char)((i+100)%251));
  CHECK(pread(fd,bytes,8,-1)==-1 && errno==EINVAL);
  CHECK(pread(fd,bytes,8,FILE_LENGTH+10)==0 && lseek(fd,0,SEEK_CUR)==17);
  unsigned char *full=mmap(0,FILE_LENGTH,PROT_READ|PROT_WRITE,MAP_PRIVATE,fd,0);
  CHECK(full!=MAP_FAILED && !((uintptr_t)full&4095));
  for(int i=0;i<FILE_LENGTH;++i)CHECK(full[i]==(unsigned char)(i%251));
  for(int i=FILE_LENGTH;i<135168;++i)CHECK(!full[i]);
  CHECK(lseek(fd,0,SEEK_CUR)==17);
  unsigned char *offset=mmap(0,8192,3,MAP_PRIVATE,fd,4096);CHECK(offset!=MAP_FAILED);
  CHECK(offset[0]==(unsigned char)(4096%251) && offset[8191]==(unsigned char)(12287%251));
  unsigned char *shortmap=mmap(0,7,3,MAP_PRIVATE,fd,0);CHECK(shortmap!=MAP_FAILED);
  CHECK(shortmap[100]==100 && shortmap[4095]==(unsigned char)(4095%251));
  full[0]=255;CHECK(pread(fd,bytes,1,0)==1 && !bytes[0]);
  CHECK(!close(fd));
  CHECK(full[FILE_LENGTH-1]==(unsigned char)((FILE_LENGTH-1)%251) && full[0]==255);
  CHECK(!munmap(full,FILE_LENGTH) && !munmap(offset,8192) && !munmap(shortmap,7));
#ifdef OWNED
  CHECK(!PrivateChecks());
#endif
  puts("private file bytes, positional reads, page tails, offsets and close-after-map: PASS");
  return 0;
}
#ifdef BLINK_MANAGED_FILEMAP
static _Thread_local unsigned char *held;
int HoldFileMapping(void) {
  if(BlinkHostMemoryBegin(1024*1024) || BlinkHostMemoryEnablePrivateFiles())return 1;
  int fd=open("image",O_RDONLY);if(fd<0)return 2;
  held=mmap(0,4096,3,MAP_PRIVATE,fd,0);
  if(held==MAP_FAILED || close(fd))return 3;
  return 0;
}
int ReleaseFileMapping(int expected) {
  if(!held || held[0]!=expected || !BlinkHostMemoryContains(held,4096))return 1;
  if(munmap(held,4096) || BlinkHostMemoryEnd())return 2;
  held=0;return 0;
}
#else
int main(int argc, char **argv) {
#ifndef BLINK_NATIVE_ADAPTER
  if(argc>1 && !strcmp(argv[1],"beyond-eof")) {
    int fd=open("image",O_RDONLY);
    unsigned char *p=mmap(0,8192,3,MAP_PRIVATE,fd,131072);
    if(p==MAP_FAILED)return 90;
    volatile unsigned char value=p[4096];(void)value;return 91;
  }
#else
  (void)argc;(void)argv;
#endif
  return FileMappingProbe();
}
#endif
