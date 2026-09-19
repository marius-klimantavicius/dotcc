#define _GNU_SOURCE
#include <signal.h>
#include <setjmp.h>
#include <stdlib.h>
#include <string.h>
#include <sys/mman.h>
#include <ucontext.h>
#include <unistd.h>
#include "corpus.h"
#include "output.h"
#if !defined(__x86_64__) || !defined(__linux__)
#error This independent reference requires Linux x86-64 ucontext.
#endif
static sigjmp_buf recovery;
static struct CpuResult result;
static unsigned char *entry;
static void Capture(int signal,siginfo_t *info,void *context) {
  ucontext_t *uc=context;
  result.ax=uc->uc_mcontext.gregs[REG_RAX];
  result.bx=uc->uc_mcontext.gregs[REG_RBX];
  result.mxcsr=uc->uc_mcontext.fpregs->mxcsr;
  result.cx=uc->uc_mcontext.gregs[REG_RCX];
  result.dx=uc->uc_mcontext.gregs[REG_RDX];
  result.flags=uc->uc_mcontext.gregs[REG_EFL];
  result.ip=uc->uc_mcontext.gregs[REG_RIP]-(uintptr_t)entry;
  result.raw_signal=signal;result.raw_code=info->si_code;
  result.signal=signal==SIGTRAP?0:signal;
  if(signal==SIGTRAP)--result.ip; /* INT3 consumed only by hardware witness. */
  const unsigned char *x=(const unsigned char *)&uc->uc_mcontext.fpregs->_xmm[0];
  for(int i=0;i<32;++i)result.xmm[i]=x[i];
  siglongjmp(recovery,1);
}
int main(int argc,char **argv) {
  if(argc!=2)return 2;
  int index=atoi(argv[1]);if(index<0||(unsigned)index>=CPU_CASES)return 2;
  const struct CpuCase *c=cpu_cases+index;
  unsigned char *code=mmap(0,2*CPU_PAGE,PROT_READ|PROT_WRITE,MAP_PRIVATE|MAP_ANONYMOUS,-1,0);
  unsigned char *data=mmap(0,2*CPU_PAGE,PROT_READ|PROT_WRITE,MAP_PRIVATE|MAP_ANONYMOUS,-1,0);
  if(code==MAP_FAILED||data==MAP_FAILED)return 3;
  CpuData(data,2*CPU_PAGE);
  if(c->data_pages==1 && mprotect(data+CPU_PAGE,CPU_PAGE,PROT_NONE))return 3;
  entry=code+c->code_offset;memcpy(entry,c->code,c->length);entry[c->length]=0xcc;
  if(mprotect(code,2*CPU_PAGE,PROT_READ|PROT_EXEC))return 3;
  struct sigaction action={0};action.sa_sigaction=Capture;action.sa_flags=SA_SIGINFO;
  sigemptyset(&action.sa_mask);
  if(sigaction(SIGTRAP,&action,0)||sigaction(SIGFPE,&action,0)||sigaction(SIGSEGV,&action,0)||sigaction(SIGILL,&action,0))return 3;
  unsigned char xmm_input[32];CpuXmm(c,xmm_input);
  unsigned mxcsr_input=CpuMxcsr(c);
  if(!sigsetjmp(recovery,1)) {
    /* Fixed inputs and code bytes are shared with the interpreter corpus.
     * No instruction under test is reimplemented in this reference. */
    __asm__ volatile("ldmxcsr %[mxcsr]\n\t"
                     "movdqu %[low], %%xmm0\n\t"
                     "movdqu %[high], %%xmm1\n\t"
                     "pushq %[flags]\n\tpopfq\n\tjmp *%[entry]"
      : : "a"(c->ax),"b"(data+c->data_offset),"c"(c->cx),"d"(c->dx),
          [flags]"r"(c->flags),[entry]"r"(entry),
          [mxcsr]"m"(mxcsr_input),[low]"m"(xmm_input[0]),[high]"m"(xmm_input[16])
      : "xmm0","xmm1","cc","memory");
    __builtin_unreachable();
  }
  result.completed=-1; /* Hardware capture does not measure retired steps. */
  CpuPrint(c,&result,data,c->data_pages*CPU_PAGE);
  return result.signal==c->fault && result.ip==(c->fault?0:c->length)?0:4;
}
