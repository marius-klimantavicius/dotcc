#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <termios.h>
#include <sys/ioctl.h>
#ifndef BLINK_MANAGED_TERMINAL
#include <fcntl.h>
#include <unistd.h>
#include <sys/socket.h>
#undef socket
int socket(int, int, int);
#endif
#define CHECK(x) do { if (!(x)) { printf("terminal failure %d\n",__LINE__); return __LINE__; } } while(0)
static uint64_t digest=14695981039346656037ull;
static void Hash(const void *memory, unsigned long size) {
  const unsigned char *bytes=memory;
  unsigned long i;
  for(i=0;i<size;++i) digest=(digest ^ bytes[i])*1099511628211ull;
}
static void HashInt(int value) { Hash(&value,sizeof(value)); }
static void HashSpeed(speed_t value) { Hash(&value,sizeof(value)); }
int SpeedProbe(void) {
  struct termios record;
  unsigned int seed,operation,code;
  digest=14695981039346656037ull;
  CHECK(sizeof(record)==60);
  for(seed=0;seed<4;++seed) for(code=0;code<8194;++code) {
    speed_t speed=code==8192 ? 0x80000000u : code==8193 ? 0xffffffffu : code;
    memset(&record,seed==0 ? 0 : seed==1 ? 255 : seed==2 ? 85 : 170,sizeof(record));
    for(operation=0;operation<4;++operation) {
      int result;
      errno=123;
      result=operation==0 ? cfsetispeed(&record,speed) : operation==1 ? cfsetospeed(&record,speed) : operation==2 ? cfsetispeed(&record,0) : cfsetospeed(&record,B115200);
      HashInt(result);HashInt(errno);Hash(&record,sizeof(record));
      HashSpeed(cfgetispeed(&record));HashSpeed(cfgetospeed(&record));
    }
  }
  printf("termios full-record speed digest %llu\n",(unsigned long long)digest);
  return 0;
}
int TerminalDescriptor(int fd, int expected) {
  struct termios record,copy;
  struct winsize size,original;
  memset(&record,0x5a,sizeof(record));memcpy(&copy,&record,sizeof(record));
  memset(&size,0x6b,sizeof(size));memcpy(&original,&size,sizeof(size));
  CHECK(ioctl(fd,TIOCGWINSZ,&size)==-1 && errno==expected && !memcmp(&size,&original,sizeof(size)));
  CHECK(tcgetattr(fd,&record)==-1 && errno==expected && !memcmp(&record,&copy,sizeof(record)));
  CHECK(tcsetattr(fd,TCSANOW,&record)==-1 && errno==expected && !memcmp(&record,&copy,sizeof(record)));
  CHECK(tcdrain(fd)==-1 && errno==expected);
  CHECK(tcflow(fd,0)==-1 && errno==expected);
  CHECK(tcflush(fd,TCIFLUSH)==-1 && errno==expected);
  CHECK(tcsendbreak(fd,0)==-1 && errno==expected);
  CHECK(tcgetpgrp(fd)==-1 && errno==expected);
  CHECK(tcsetpgrp(fd,1)==-1 && errno==expected);
  CHECK(tcgetsid(fd)==-1 && errno==expected);
  return 0;
}
#ifdef BLINK_MANAGED_TERMINAL
static int effects;
static void *Payload(void) { ++effects; return (void *)(uintptr_t)1; }
int TerminalPrivate(int fd) {
  effects=0;
  CHECK(ioctl(fd,TIOCGWINSZ,Payload())==-1 && errno==ENOTTY && effects==1);
  CHECK(ioctl(fd,TIOCGWINSZ)==-1 && errno==ENOTTY);
  CHECK(ioctl(fd,0xfffffffful,Payload())==-1 && errno==ENOTTY && effects==2);
  CHECK(tcgetattr(fd,(struct termios *)Payload())==-1 && errno==ENOTTY && effects==3);
  CHECK(tcsetattr(fd,123,(struct termios *)Payload())==-1 && errno==ENOTTY && effects==4);
  CHECK(tcgetattr(-1,0)==-1 && errno==EBADF);
  CHECK(tcsetattr(-1,123,0)==-1 && errno==EBADF);
  CHECK(cfgetispeed(0)==(speed_t)-1 && errno==EFAULT);
  CHECK(cfgetospeed(0)==(speed_t)-1 && errno==EFAULT);
  CHECK(cfsetispeed(0,B9600)==-1 && errno==EFAULT);
  CHECK(cfsetospeed(0,B9600)==-1 && errno==EFAULT);
  return 0;
}
#else
int main(void) {
  int descriptors[2],file,sock,result;
  char name[]="/tmp/blink-terminal-XXXXXX";
  file=mkstemp(name);CHECK(file>=0);CHECK(!unlink(name));
  CHECK(!pipe(descriptors));sock=socket(AF_INET,SOCK_STREAM,0);CHECK(sock>=0);
  CHECK(!TerminalDescriptor(file,ENOTTY));CHECK(!TerminalDescriptor(descriptors[0],ENOTTY));
  CHECK(!TerminalDescriptor(descriptors[1],ENOTTY));CHECK(!TerminalDescriptor(sock,ENOTTY));
  CHECK(!TerminalDescriptor(-1,EBADF));CHECK(!close(file));CHECK(!TerminalDescriptor(file,EBADF));
  CHECK(!close(descriptors[0]) && !close(descriptors[1]) && !close(sock));
  result=SpeedProbe();if(result)return result;
  puts("nonterminal descriptor policy: PASS");return 0;
}
#endif
