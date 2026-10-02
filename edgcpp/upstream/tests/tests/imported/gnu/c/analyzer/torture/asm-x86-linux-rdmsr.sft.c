//type: fp
//options: 
# 0 "./analyzer/torture/asm-x86-linux-rdmsr.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/torture/asm-x86-linux-rdmsr.c"



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
# 5 "./analyzer/torture/asm-x86-linux-rdmsr.c" 2
# 18 "./analyzer/torture/asm-x86-linux-rdmsr.c"
static unsigned long long __rdmsr(unsigned int msr)
{
 unsigned long long low, high;

 asm volatile("1: rdmsr\n"
       "2:\n"
       : "=a" (low), "=d" (high) : "c" (msr));

 return ((low) | (high) << 32);
}

void test (void)
{
  __analyzer_eval (__rdmsr (0));
  __analyzer_eval (__rdmsr (1));
}
