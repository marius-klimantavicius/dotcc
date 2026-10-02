//type: fp
//options: 
# 0 "./analyzer/asm-x86-lp64-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/asm-x86-lp64-1.c"



# 1 "./analyzer/analyzer-decls.h" 1
# 20 "./analyzer/analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 5 "./analyzer/asm-x86-lp64-1.c" 2

# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 1 3 4
# 9 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 3 4
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/usr/include/stdint.h" 1 3 4
# 25 "/usr/include/stdint.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 26 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wchar.h" 1 3 4
# 22 "/usr/include/bits/wchar.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/wchar.h" 2 3 4
# 27 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/stdint.h" 2 3 4
# 36 "/usr/include/stdint.h" 3 4
typedef signed char int8_t;
typedef short int int16_t;
typedef int int32_t;

typedef long int int64_t;







typedef unsigned char uint8_t;
typedef unsigned short int uint16_t;

typedef unsigned int uint32_t;



typedef unsigned long int uint64_t;
# 65 "/usr/include/stdint.h" 3 4
typedef signed char int_least8_t;
typedef short int int_least16_t;
typedef int int_least32_t;

typedef long int int_least64_t;






typedef unsigned char uint_least8_t;
typedef unsigned short int uint_least16_t;
typedef unsigned int uint_least32_t;

typedef unsigned long int uint_least64_t;
# 90 "/usr/include/stdint.h" 3 4
typedef signed char int_fast8_t;

typedef long int int_fast16_t;
typedef long int int_fast32_t;
typedef long int int_fast64_t;
# 103 "/usr/include/stdint.h" 3 4
typedef unsigned char uint_fast8_t;

typedef unsigned long int uint_fast16_t;
typedef unsigned long int uint_fast32_t;
typedef unsigned long int uint_fast64_t;
# 119 "/usr/include/stdint.h" 3 4
typedef long int intptr_t;


typedef unsigned long int uintptr_t;
# 134 "/usr/include/stdint.h" 3 4
typedef long int intmax_t;
typedef unsigned long int uintmax_t;
# 12 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 2 3 4
#pragma GCC diagnostic pop
# 7 "./analyzer/asm-x86-lp64-1.c" 2


# 8 "./analyzer/asm-x86-lp64-1.c"
int test_1 (int src)
{
  int dst;
  asm ("mov %1, %0\n\t"
       "add $1, %0"
       : "=r" (dst)
       : "r" (src));
  return dst;
}

uint32_t test_2 (uint32_t Mask)
{
  uint32_t Index;
  asm ("bsfl %[aMask], %[aIndex]"
       : [aIndex] "=r" (Index)
       : [aMask] "r" (Mask)
       : "cc");
  return Index;
}

int test_3a (int p1, int p2)
{
  asm goto ("btl %1, %0\n\t"
     "jc %l2"
     :
     : "r" (p1), "r" (p2)
     : "cc"
     : carry);

  return 0;

 carry:
  return 1;
}

int test_3b (int p1, int p2)
{
  asm goto ("btl %1, %0\n\t"
     "jc %l[carry]"
     :
     : "r" (p1), "r" (p2)
     : "cc"
     : carry);

  return 0;

 carry:
  return 1;
}

uint64_t test_4 (void)
{
  uint64_t start_time, end_time;


  asm volatile ("rdtsc\n\t"
  "shl $32, %%rdx\n\t"
  "or %%rdx, %0"
  : "=a" (start_time)
  :
  : "rdx");




  asm volatile ("rdtsc\n\t"
  "shl $32, %%rdx\n\t"
  "or %%rdx, %0"
  : "=a" (end_time)
  :
  : "rdx");

  __analyzer_eval (start_time == end_time);


  return end_time - start_time;
}

static uint64_t get_time (void)
{
  uint64_t result;
  asm volatile ("rdtsc\n\t"
  "shl $32, %%rdx\n\t"
  "or %%rdx, %0"
  : "=a" (result)
  :
  : "rdx");
  return result;
}

uint64_t test_4a (void)
{
  uint64_t start_time, end_time;

  start_time = get_time ();

  end_time = get_time ();

  __analyzer_eval (start_time == end_time);


  return end_time - start_time;
}

asm ("\t.pushsection .text\n"
     "\t.globl add_asm\n"
     "\t.type add_asm, @function\n"
     "add_asm:\n"
     "\tmovq %rdi, %rax\n"
     "\tadd %rsi, %rax\n"
     "\tret\n"
     "\t.popsection\n");

int test_5 (int count)
{
  asm goto ("dec %0; jb %l[stop]"
     : "+r" (count)
     :
     :
     : stop);
  return count;
stop:
  return 0;
}
