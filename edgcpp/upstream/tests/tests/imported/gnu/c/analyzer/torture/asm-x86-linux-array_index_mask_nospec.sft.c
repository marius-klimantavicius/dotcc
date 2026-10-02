//type: fp
//options: 
# 0 "./analyzer/torture/asm-x86-linux-array_index_mask_nospec.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/torture/asm-x86-linux-array_index_mask_nospec.c"




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
# 6 "./analyzer/torture/asm-x86-linux-array_index_mask_nospec.c" 2



static inline unsigned long array_index_mask_nospec(unsigned long index,
          unsigned long size)
{
 unsigned long mask;

 asm volatile ("cmp %1,%2; sbb %0,%0;"
   :"=r" (mask)
   :"g"(size),"r" (index)
   :"cc");
 return mask;
}




void test_1 (unsigned long index, unsigned long size)
{
  unsigned long a = array_index_mask_nospec (index, size);
  unsigned long b = array_index_mask_nospec (index, size);
  __analyzer_eval (a == b);
}

void test_2 (unsigned long index_a, unsigned long size_a,
      unsigned long index_b, unsigned long size_b)
{
  unsigned long aa_1 = array_index_mask_nospec (index_a, size_a);
  unsigned long ab_1 = array_index_mask_nospec (index_a, size_b);
  unsigned long ba_1 = array_index_mask_nospec (index_b, size_a);
  unsigned long bb_1 = array_index_mask_nospec (index_b, size_b);

  unsigned long aa_2 = array_index_mask_nospec (index_a, size_a);
  unsigned long ab_2 = array_index_mask_nospec (index_a, size_b);
  unsigned long ba_2 = array_index_mask_nospec (index_b, size_a);
  unsigned long bb_2 = array_index_mask_nospec (index_b, size_b);

  __analyzer_eval (aa_1 == aa_2);
  __analyzer_eval (ab_1 == ab_2);
  __analyzer_eval (ba_1 == ba_2);
  __analyzer_eval (bb_1 == bb_2);

  __analyzer_eval (aa_1 == ab_1);
  __analyzer_eval (aa_1 == ba_1);
  __analyzer_eval (aa_1 == bb_1);

  __analyzer_eval (ab_1 == ba_1);
  __analyzer_eval (ab_1 == bb_1);

  __analyzer_eval (ba_1 == bb_1);
}




void test_3 (unsigned long index, unsigned long size)
{
  unsigned long a = array_index_mask_nospec (index, size);
  unsigned long b;


  asm volatile ("cmp %1,%2; sbb %0,%0;"
  :"=r" (b)
  :"g"(size),"r" (index)
  :"cc");

  __analyzer_eval (a == b);
}
