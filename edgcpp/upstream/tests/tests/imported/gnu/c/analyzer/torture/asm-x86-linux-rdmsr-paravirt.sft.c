//type: fp
//options: 
# 0 "./analyzer/torture/asm-x86-linux-rdmsr-paravirt.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/torture/asm-x86-linux-rdmsr-paravirt.c"
# 9 "./analyzer/torture/asm-x86-linux-rdmsr-paravirt.c"
# 1 "./analyzer/torture/../analyzer-decls.h" 1
# 20 "./analyzer/torture/../analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./analyzer/torture/../analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 10 "./analyzer/torture/asm-x86-linux-rdmsr-paravirt.c" 2

typedef unsigned char u8;
typedef unsigned int u32;
typedef unsigned long int u64;
typedef long unsigned int size_t;
# 42 "./analyzer/torture/asm-x86-linux-rdmsr-paravirt.c"
register unsigned long current_stack_pointer asm("rsp");
# 67 "./analyzer/torture/asm-x86-linux-rdmsr-paravirt.c"
struct pv_cpu_ops {

  u64 (*read_msr_safe)(unsigned int msr, int *err);

};

struct paravirt_patch_template {
  struct pv_cpu_ops cpu;

};
extern struct paravirt_patch_template pv_ops;
# 184 "./analyzer/torture/asm-x86-linux-rdmsr-paravirt.c"
static inline u64 paravirt_read_msr_safe(unsigned msr, int *err)
{
 return ({ unsigned long __edi = __edi, __esi = __esi, __edx = __edx, __ecx = __ecx, __eax = __eax;; ((void)pv_ops.cpu.read_msr_safe); asm volatile("771:\n\t" "999:\n\t" ".pushsection .discard.retpoline_safe\n\t" " " ".quad" " " " 999b\n\t" ".popsection\n\t" "call *%c[paravirt_opptr];" "\n" "772:\n" ".pushsection .parainstructions,\"a\"\n" " " ".balign 8" " " "\n" " " ".quad" " " " 771b\n" "  .byte " "%c[paravirt_typenum]" "\n" "  .byte 772b-771b\n" "  .short " "%c[paravirt_clobber]" "\n" ".popsection\n" : "=D" (__edi), "=S" (__esi), "=d" (__edx), "=c" (__ecx), "=a" (__eax), "+r" (current_stack_pointer) : [paravirt_typenum] "i" ((((size_t)&((struct paravirt_patch_template *)0)->cpu.read_msr_safe) / sizeof(void *))), [paravirt_opptr] "i" (&(pv_ops.cpu.read_msr_safe)), [paravirt_clobber] "i" (((1 << 9) - 1)), "D" ((unsigned long)(msr)), "S" ((unsigned long)(err)) : "memory", "cc" , "r8", "r9", "r10", "r11"); ({ unsigned long __mask = ~0UL; switch (sizeof(u64)) { case 1: __mask = 0xffUL; break; case 2: __mask = 0xffffUL; break; case 4: __mask = 0xffffffffUL; break; default: break; } __mask & __eax; }); });
}
# 199 "./analyzer/torture/asm-x86-linux-rdmsr-paravirt.c"
void check_init_int(int);
void check_init_u32(u32);

void test(void)
{
  int err;
  u32 eax, edx;
  err = ({ int _err; u64 _l = paravirt_read_msr_safe(0, &_err); (*&eax) = (u32)_l; (*&edx) = _l >> 32; _err; });
  check_init_int(err);
  check_init_u32(eax);
  check_init_u32(edx);
}
