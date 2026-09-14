#define _GNU_SOURCE 1
#include <signal.h>
#include <stdint.h>
#include <string.h>
static struct sigaction saved;
static uint64_t savedmask;
int NativeActionGuard(int check) {
  struct sigaction current;
  sigset_t mask;
  uint64_t low;
  if(sigaction(SIGUSR1,0,&current) || sigprocmask(SIG_SETMASK,0,&mask))return 1;
  memcpy(&low,&mask,8);
  if(!check){saved=current;savedmask=low;return 0;}
  return saved.sa_handler!=current.sa_handler || saved.sa_flags!=current.sa_flags ||
      saved.sa_restorer!=current.sa_restorer || memcmp(&saved.sa_mask,&current.sa_mask,8) || savedmask!=low;
}
