#include <errno.h>
#include <stdio.h>
#include <unistd.h>
#include <sys/wait.h>
#ifdef BLINK_MANAGED_PROCESS
#include "host-errors.h"
#include "host-identity.h"
#include "host-process-policy.h"
static _Thread_local int evaluations;
static char *SelectExecutable(void) { ++evaluations; return "/bin/true"; }
static char **Args(void) { static char *args[2]={"true",0};++evaluations;return args; }
int ProcessPolicy(int bound) {
  int status=0x12345678, before=getpid();
  int unsupported=bound?ENOSYS:ENODEV, denied=bound?EPERM:ENODEV;
  int (*indirect_fork)(void)=fork;
  errno=0;if(fork()!=-1 || errno!=unsupported || indirect_fork()!=-1 || errno!=unsupported)return 1;
  evaluations=0;errno=0;
  if(execv(SelectExecutable(),Args())!=-1 || errno!=unsupported || evaluations!=2)return 2;
  if(execvp("true",0)!=-1 || errno!=unsupported || execve("/bin/true",0,0)!=-1 || errno!=unsupported)return 3;
  errno=0;if(waitpid(-1,&status,0)!=-1 || errno!=(bound?ECHILD:ENODEV) || status!=0x12345678)return 4;
  if(waitpid(123,&status,0x400)!=-1 || errno!=(bound?EINVAL:ENODEV) || status!=0x12345678)return 5;
  if(setuid(0)!=-1 || errno!=denied || seteuid(123)!=-1 || errno!=denied || setgid(0)!=-1 || errno!=denied || setegid(456)!=-1 || errno!=denied)return 6;
  if(setpgid(0,0)!=-1 || errno!=denied || setsid()!=-1 || errno!=denied)return 7;
  if(bound && (getpid()!=before || getuid()!=0 || getgid()!=0))return 8;
  return 0;
}
int main(void) { return ProcessPolicy(1); }
#else
int main(void) {
  int status;int child=fork();if(child<0)return 1;
  if(!child)_exit(37);
  if(waitpid(child,&status,0)!=child || !WIFEXITED(status) || WEXITSTATUS(status)!=37)return 2;
  puts("native fork/wait baseline: PASS");return 0;
}
#endif
