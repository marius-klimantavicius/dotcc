/* Actual upstream types. The native profile changes only the explicit jump
 * storage; other authored host records have separate native layout probes. */
#if defined(BLINK_NATIVE_PROFILE)
#include <setjmp.h>
#include <signal.h>
#include "abi.h"
typedef blink_host_signal_jump_storage core_profile_sigjmp_buf[1];
#define sigjmp_buf core_profile_sigjmp_buf
#endif
#include <stdio.h>
#include <stddef.h>
#include <string.h>
#include "blink/machine.h"
#include "blink/endian.h"

/* Link storage referenced by header-only inline helpers. This probe never
 * calls those memory helpers; it does not supply a mapping implementation. */
bool FLAG_nolinear = true;
struct HostPages g_hostpages;

#define TYPE(T) do { static T pair[2]; static struct { char prefix; T item; } wrap; \
  printf(#T ".size %zu\n", sizeof(T)); \
  printf(#T ".align %zu\n", _Alignof(T)); \
  printf(#T ".placement %zu\n", (size_t)((char *)&wrap.item-(char *)&wrap)); \
  printf(#T ".stride %zu\n", (size_t)((char *)&pair[1]-(char *)&pair[0])); } while(0)
#define FIELD(T,F) do { static T value; printf(#T "." #F ".offset %zu\n", offsetof(T,F)); \
  printf(#T "." #F ".actual %zu\n", (size_t)((char *)&value.F-(char *)&value)); \
  printf(#T "." #F ".size %zu\n", sizeof(value.F)); } while(0)

int main(void) {
  TYPE(struct Machine); TYPE(struct System); TYPE(struct MachineState);
  TYPE(struct DescriptorCache); TYPE(struct OpCache); TYPE(struct MachineTlb);
  TYPE(struct Fd); TYPE(struct Fds); TYPE(struct FdCb); TYPE(struct Elf);
  TYPE(Elf64_Ehdr_); TYPE(Elf64_Phdr_); TYPE(Elf64_Shdr_); TYPE(Elf64_Sym_);
  TYPE(struct iovec_linux); TYPE(struct pollfd_linux); TYPE(struct timespec_linux);
  TYPE(struct sigaction_linux); TYPE(struct sigaltstack_linux);
  TYPE(struct sockaddr_in_linux); TYPE(struct stat_linux); TYPE(struct rusage_linux);
  TYPE(struct utsname_linux); TYPE(struct rlimit_linux); TYPE(struct msghdr_linux);
  FIELD(struct Machine,ip); FIELD(struct Machine,mode); FIELD(struct Machine,flags);
  FIELD(struct Machine,ax); FIELD(struct Machine,ah); FIELD(struct Machine,bx);
  FIELD(struct Machine,r15); FIELD(struct Machine,weg); FIELD(struct Machine,xmm);
  FIELD(struct Machine,xedd); FIELD(struct Machine,fs); FIELD(struct Machine,gs);
  FIELD(struct Machine,fpu); FIELD(struct Machine,mxcsr); FIELD(struct Machine,system);
  FIELD(struct Machine,signals); FIELD(struct Machine,sigmask); FIELD(struct Machine,canhalt);
  FIELD(struct Machine,tlb); FIELD(struct Machine,onhalt); FIELD(struct Machine,sigaltstack);
  FIELD(struct Machine,spawn_sigmask); FIELD(struct Machine,opcache);
  FIELD(struct System,mode); FIELD(struct System,real); FIELD(struct System,cr3);
  FIELD(struct System,brk); FIELD(struct System,jit); FIELD(struct System,fds);
  FIELD(struct System,elf); FIELD(struct System,exec_sigmask); FIELD(struct System,hands);
  FIELD(struct System,rlim); FIELD(struct System,onfilemap); FIELD(struct System,exec);
  FIELD(struct Fd,fildes); FIELD(struct Fd,elem); FIELD(struct Fd,cb); FIELD(struct Fd,saddr);
  FIELD(Elf64_Ehdr_,entry); FIELD(Elf64_Ehdr_,phoff); FIELD(Elf64_Phdr_,vaddr);
  FIELD(Elf64_Phdr_,memsz); FIELD(struct stat_linux,size);
  static struct Machine m;
  printf("register.machine_alignment %zu\n",(size_t)&m % _Alignof(struct Machine));
  printf("register.vector_alignment %zu\n",(size_t)&m.xmm % 16);
  memset(&m,0,sizeof(m));
  Write64(m.ax,0x1122334455667788ul);
  m.ah=0xab;
  Write64(m.r15,0x8877665544332211ul);
  for (int i=0;i<16;i++) m.xmm[3][i]=(unsigned char)(i*11);
  printf("register.alias %lx %u %u %lx\n",(unsigned long)Read64(m.weg[0]),m.al,m.ah,(unsigned long)Read64(m.weg[15]));
  printf("register.vector");
  for (int i=0;i<16;i++) printf(" %u",m.xmm[3][i]);
  printf("\n");
  return 0;
}
