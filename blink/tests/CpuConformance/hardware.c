#define _GNU_SOURCE
#include <stdlib.h>
#include <string.h>
#include <sys/mman.h>
#include "corpus.h"
#include "output.h"
#if !defined(__x86_64__) || !defined(__linux__)
#error This independent reference requires Linux x86-64 SysV.
#endif
struct HardwareState {
  uint64_t ax,bx,cx,dx,flags;
  unsigned mxcsr;
  unsigned char xmm[32];
  uint64_t ip;
};
_Static_assert(offsetof(struct HardwareState,ax)==0,"AX capture offset");
_Static_assert(offsetof(struct HardwareState,bx)==8,"BX capture offset");
_Static_assert(offsetof(struct HardwareState,cx)==16,"CX capture offset");
_Static_assert(offsetof(struct HardwareState,dx)==24,"DX capture offset");
_Static_assert(offsetof(struct HardwareState,flags)==32,"flags capture offset");
_Static_assert(offsetof(struct HardwareState,mxcsr)==40,"MXCSR capture offset");
_Static_assert(offsetof(struct HardwareState,xmm)==44,"XMM capture offset");
_Static_assert(offsetof(struct HardwareState,ip)==80,"IP capture offset");
extern void CpuHardwareRun(const struct HardwareState *,struct HardwareState *,void *);
int main(int argc,char **argv) {
  if(argc!=2)return 2;
  int index=atoi(argv[1]);if(index<0||(unsigned)index>=CPU_CASES)return 2;
  const struct CpuCase *c=cpu_cases+index;
  if(c->fault)return 2; /* Historical custom fault rows are never executed. */
  unsigned char *code=mmap(0,2*CPU_PAGE,PROT_READ|PROT_WRITE,MAP_PRIVATE|MAP_ANONYMOUS,-1,0);
  unsigned char *data=mmap(0,2*CPU_PAGE,PROT_READ|PROT_WRITE,MAP_PRIVATE|MAP_ANONYMOUS,-1,0);
  if(code==MAP_FAILED||data==MAP_FAILED)return 3;
  CpuData(data,2*CPU_PAGE);
  unsigned char *entry=code+c->code_offset;
  memcpy(entry,c->code,c->length);
  /* LEA R15,[RIP-7] observes the address immediately after the corpus bytes;
   * RET returns to our assembly capture. Neither instruction changes flags. */
  const unsigned char completion[]={0x4c,0x8d,0x3d,0xf9,0xff,0xff,0xff,0xc3};
  memcpy(entry+c->length,completion,sizeof(completion));
  if(mprotect(code,2*CPU_PAGE,PROT_READ|PROT_EXEC))return 3;
  struct HardwareState input={0},observed={0};
  input.ax=c->ax;input.bx=(uintptr_t)(data+c->data_offset);input.cx=c->cx;
  input.dx=c->dx;input.flags=c->flags;input.mxcsr=CpuMxcsr(c);CpuXmm(c,input.xmm);
  CpuHardwareRun(&input,&observed,entry);
  struct CpuResult result={0};
  result.ax=observed.ax;result.bx=observed.bx;result.cx=observed.cx;result.dx=observed.dx;
  result.flags=observed.flags;result.mxcsr=observed.mxcsr;
  result.ip=observed.ip-(uintptr_t)entry;memcpy(result.xmm,observed.xmm,32);
  result.completed=-1; /* Native completion does not count retired instructions. */
  CpuPrint(c,&result,data,c->data_pages*CPU_PAGE);
  int okay=result.ip==c->length;
  munmap(code,2*CPU_PAGE);munmap(data,2*CPU_PAGE);
  return okay?0:4;
}
