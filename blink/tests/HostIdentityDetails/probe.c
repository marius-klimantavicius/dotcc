#define _GNU_SOURCE
#include <errno.h>
#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_IDENTITY
#include "host-identity.h"
#endif
int Details(void) {
  unsigned real=99, effective=98, saved=97;
  char name[256];
  errno=123;
  if(getresuid(&real,&effective,&saved) || real!=getuid() || effective!=geteuid() || errno!=123)return 1;
  if(getresgid(&real,&effective,&saved) || real!=getgid() || effective!=getegid())return 2;
  if(getgroups(0,0)<0)return 3;
  errno=0; if(getgroups(-1,0)!=-1 || errno!=EINVAL)return 4;
  if(getpgid(0)<=0 || getpgid(0)!=getpgid(getpid()) || getsid(0)<=0 || getsid(0)!=getsid(getpid()))return 5;
  errno=123; if(gethostname(name,sizeof(name)) || !name[0] || errno!=123)return 6;
  errno=0; if(gethostname(name,1)!=-1 || errno!=ENAMETOOLONG)return 7;
  if(sysconf(_SC_CLK_TCK)<=0 || sysconf(_SC_NGROUPS_MAX)<0 || sysconf(_SC_PAGESIZE)<=0)return 8;
  errno=0; if(sysconf(-1)!=-1 || errno!=EINVAL)return 9;
  puts("process metadata: credential triples, group query, process/session queries, hostname and limits: PASS");
  return 0;
}
#ifdef BLINK_MANAGED_IDENTITY
int PrivateDetails(int pid, const char *expected) {
  unsigned a=7,b=8,c=9; char name[80];
  if(getpgid(0)!=pid || getsid(0)!=pid || getgroups(1,&a)!=0 || a!=7)return 1;
  if(sysconf(_SC_CLK_TCK)!=100 || sysconf(_SC_NGROUPS_MAX)!=0 || sysconf(_SC_PAGESIZE)!=4096)return 2;
  if(gethostname(name,sizeof(name)) || strcmp(name,expected))return 3;
  errno=0; if(getresuid(&a,0,&c)!=-1 || errno!=EFAULT || a!=7 || c!=9)return 4;
  errno=0; if(getpgid(pid+1)!=-1 || errno!=ESRCH)return 5;
  memset(name,0x55,sizeof(name)); errno=0;
  if(gethostname(name,1)!=-1 || errno!=ENAMETOOLONG || name[0]!=0x55)return 6;
  if(getresuid(&a,&b,&c) || a || b || c)return 7;
  return 0;
}
int UnboundDetails(void) {
  char name[80]; unsigned a=7,b=8,c=9;
  errno=0;
  if(getresgid(&a,&b,&c)!=-1 || errno!=ENODEV || a!=7 || b!=8 || c!=9)return 1;
  if(gethostname(name,sizeof(name))!=-1 || getgroups(0,0)!=-1 || getsid(0)!=-1 || sysconf(_SC_CLK_TCK)!=-1)return 2;
  return 0;
}
#endif
int main(void) { return Details(); }
