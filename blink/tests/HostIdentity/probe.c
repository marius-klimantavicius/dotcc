#include <errno.h>
#include <stdio.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_IDENTITY
#include "host-identity.h"
#endif
int Pid(void) { return getpid(); }
int Parent(void) { return getppid(); }
unsigned User(void) { return getuid(); }
unsigned Group(void) { return getgid(); }
int main(void) {
  if(getpid()<=0 || getppid()<0 || getuid()!=geteuid() || getgid()!=getegid())return 1;
  if(sizeof(getpid())!=4 || sizeof(getuid())!=4)return 2;
  errno=123;
  if(getpid()<=0 || errno!=123)return 3;
  puts("private identity: valid process IDs, matching real/effective credentials, 32-bit C values: PASS");
  return 0;
}
