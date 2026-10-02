//type: fp
//options: 
# 0 "./analyzer/torture/asm-x86-linux-cpuid.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/torture/asm-x86-linux-cpuid.c"



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
# 5 "./analyzer/torture/asm-x86-linux-cpuid.c" 2

typedef unsigned int u32;
typedef unsigned long int u64;

extern void check_init_u32 (u32 v);
extern void check_init_u64 (u32 v);



static inline void native_cpuid(unsigned int *eax, unsigned int *ebx,
    unsigned int *ecx, unsigned int *edx)
{

 asm volatile("cpuid"
     : "=a" (*eax),
       "=b" (*ebx),
       "=c" (*ecx),
       "=d" (*edx)
     : "0" (*eax), "2" (*ecx)
     : "memory");
}

static inline void cpuid(unsigned int op,
    unsigned int *eax, unsigned int *ebx,
    unsigned int *ecx, unsigned int *edx)
{
 *eax = op;
 *ecx = 0;
 native_cpuid(eax, ebx, ecx, edx);
}

void test_1 (void)
{
  u32 eax, ebx, ecx, edx;
  cpuid(0x8000001e, &eax, &ebx, &ecx, &edx);


  check_init_u32 (eax);
  check_init_u32 (ebx);
  check_init_u32 (ecx);
  check_init_u32 (edx);
}
