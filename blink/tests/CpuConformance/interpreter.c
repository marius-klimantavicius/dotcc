#include <stdlib.h>
#include <string.h>
#include "blink/bus.h"
#include "blink/endian.h"
#include "blink/flags.h"
#include "blink/machine.h"
#include "blink/map.h"
#include "blink/signal.h"
#include "corpus.h"
#include "output.h"
static int observed_signal,observed_code;
void TerminateSignal(struct Machine *m,int signal,int code) {
  (void)m;observed_signal=signal;observed_code=code;
}
/* Reusable corpus entry for a later real translated-core consumer. It still
 * requires that consumer's qualified owner lifecycle and host bindings. */
int CpuInterpreterCase(int index) {
  if(index<0||index>=CPU_CASES)return 2;
  const struct CpuCase *c=cpu_cases+index;
  struct System *s=NewSystem(XED_MACHINE_MODE_LONG);if(!s)return 3;
  struct Machine *m=NewMachine(s,0);if(!m){FreeSystem(s);return 3;}
  g_machine=m;
  s->cr0=CR0_PE|CR0_MP|CR0_ET|CR0_PG;s->cr3=AllocatePageTable(s);
  unsigned char data[2*CPU_PAGE],code[16];CpuData(data,sizeof(data));
  memcpy(code,c->code,c->length);
  if(ReserveVirtual(s,0x400000,2*CPU_PAGE,PAGE_U|PAGE_RW,-1,0,0,0)==-1 ||
     ReserveVirtual(s,0x600000,c->data_pages*CPU_PAGE,PAGE_U|PAGE_RW|PAGE_XD,-1,0,0,0)==-1 ||
     CopyToUser(m,0x400000+c->code_offset,code,c->length) ||
     CopyToUser(m,0x600000,data,c->data_pages*CPU_PAGE)) {FreeMachine(m);return 3;}
  m->ip=0x400000+c->code_offset;
  Write64(m->ax,c->ax);Write64(m->cx,c->cx);Write64(m->dx,c->dx);
  Write64(m->bx,0x600000+c->data_offset);ImportFlags(m,c->flags);
  memcpy(m->xmm,cpu_xmm,sizeof(cpu_xmm));m->mxcsr=0x1f80;
  observed_signal=observed_code=0;
  volatile unsigned completed=0;
  int halt=sigsetjmp(m->onhalt,1);
  if(!halt){m->canhalt=true;while(completed<c->steps){ExecuteInstruction(m);++completed;}}
  m->canhalt=false;
  struct CpuResult r={0};r.ax=Read64(m->ax);r.cx=Read64(m->cx);r.dx=Read64(m->dx);
  r.flags=ExportFlags(m->flags);r.ip=m->ip-(0x400000+c->code_offset);
  r.signal=r.raw_signal=observed_signal;r.raw_code=observed_code;r.halt=halt;r.completed=completed;
  memcpy(r.xmm,m->xmm,sizeof(r.xmm));
  if(CopyFromUser(m,data,0x600000,c->data_pages*CPU_PAGE)){FreeMachine(m);return 3;}
  CpuPrint(c,&r,data,c->data_pages*CPU_PAGE);
  FreeMachine(m);
  int expected=c->fault==8?kMachineDivideError:c->fault==11?kMachineSegmentationFault:0;
  return r.signal==c->fault && halt==expected && completed==(c->fault?0:c->steps)?0:4;
}
int main(int argc,char **argv) {
  if(argc!=2)return 2;
  InitMap();InitBus();return CpuInterpreterCase(atoi(argv[1]));
}
