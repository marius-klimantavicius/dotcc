#define _GNU_SOURCE 1
#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/uio.h>
#ifdef BLINK_MANAGED_IO
#include "host-io.h"
#endif
int FileProbe(void) {
  char buffer[16]={0};
  int fd=open("seed",O_RDONLY);
  if(fd<0 || read(fd,buffer,2)!=2 || memcmp(buffer,"ab",2)) return 1;
  int copy=dup(fd);
  if(copy<0 || read(copy,buffer,2)!=2 || memcmp(buffer,"cd",2)) return 2;
  if(close(fd) || read(copy,buffer,2)!=2 || memcmp(buffer,"ef",2) || close(copy)) return 3;
  int mode=0600;
  fd=open("created",O_RDWR|O_CREAT|O_EXCL,mode++);
  if(mode!=0601)return 12;
  if(fd<0) return 4;
  struct iovec output[3]={{"xy",2},{"",0},{"z123",4}};
  if(writev(fd,output,3)!=6 || lseek(fd,0,SEEK_SET)!=0) return 5;
  struct iovec input[2]={{buffer,1},{buffer+1,7}};
  if(readv(fd,input,2)!=6 || memcmp(buffer,"xyz123",6)) return 6;
  if(lseek(fd,2,SEEK_END)!=8 || write(fd,"!",1)!=1 || lseek(fd,6,SEEK_SET)!=6) return 7;
  if(read(fd,buffer,3)!=3 || buffer[0] || buffer[1] || buffer[2]!='!' || close(fd)) return 8;
  if(open("missing",O_RDONLY)!=-1 || errno!=ENOENT) return 9;
  if(read(-1,buffer,1)!=-1 || errno!=EBADF) return 10;
  if(open("created",O_CREAT|O_EXCL|O_WRONLY,0600)!=-1 || errno!=EEXIST) return 11;
  return 0;
}
int StreamProbe(void) {
  char buffer[8]; struct iovec v[2]={{"one",3},{"two",3}};
  if(read(0,buffer,3)!=3 || memcmp(buffer,"in!",3))return 1;
  if(writev(1,v,2)!=6 || write(2,"err",3)!=3)return 2;
  return 0;
}
int MissingProbe(void) { return open("/etc/passwd",O_RDONLY); }
int LargeProbe(void) {
  unsigned char *buffer=malloc(131072), *copy=malloc(131072);
  if(!buffer || !copy)return 1;
  for(int i=0;i<131072;i++)buffer[i]=(unsigned char)(i*17+3);
  int fd=open("large",O_RDWR|O_CREAT|O_EXCL,0600);
  if(fd<0)return 2;
  struct iovec v[2]={{buffer,65535},{buffer+65535,65537}};
  long n=writev(fd,v,2);
  if(n<=0 || n>131072)return 3;
  while(n<131072) { long k=write(fd,buffer+n,131072-n); if(k<=0)return 4; n+=k; }
  if(lseek(fd,0,SEEK_SET))return 5;
  v[0].iov_base=copy;v[1].iov_base=copy+65535;
  n=readv(fd,v,2);
  if(n<=0 || n>131072)return 6;
  while(n<131072) { long k=read(fd,copy+n,131072-n); if(k<=0)return 7; n+=k; }
  int failed=memcmp(buffer,copy,131072)!=0;
  free(buffer);free(copy);
  return close(fd) || failed;
}
int main(void) {
  int result=FileProbe();
  if(result)return result;
  if(LargeProbe())return 20;
  puts("private file C operations: shared cursors, vectors, sparse bytes, errors: PASS");
  return 0;
}
