#include <stdlib.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_TERMINATION
#include "host-termination.h"
#endif
void RunExit(int kind,int status) {
  if(kind==0)exit(status);
  if(kind==1)_Exit(status);
  if(kind==2)_exit(status);
  abort();
}
void ThroughPointer(int kind,int status) {
  if(kind==3) { void (*fn)(void)=abort;fn(); }
  else { void (*fn)(int)=kind==0 ? exit : kind==1 ? _Exit : _exit;fn(status); }
}
int main(int argc,char **argv) {
  if(argc!=3)return 99;
  int kind=atoi(argv[1]),indirect=atoi(argv[2]);
  if(indirect)ThroughPointer(kind,37);else RunExit(kind,37);
  return 99;
}
