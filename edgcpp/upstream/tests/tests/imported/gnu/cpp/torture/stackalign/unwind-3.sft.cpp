//type: rp
//options: 
# 0 "./torture/stackalign/unwind-3.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/stackalign/unwind-3.C"


# 1 "./torture/stackalign/test-unwind.h" 1
# 1 "./torture/stackalign/check.h" 1
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 425 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
} max_align_t;






  typedef decltype(nullptr) nullptr_t;
# 2 "./torture/stackalign/check.h" 2






# 7 "./torture/stackalign/check.h"
extern "C" void abort (void);




int
check_int (int *i, int align)
{
  *i = 20;
  if ((((ptrdiff_t) i) & (align - 1)) != 0)
    {



      abort ();
    }
  return *i;
}

void
check (void *p, int align)
{
  if ((((ptrdiff_t) p) & (align - 1)) != 0)
    {



      abort ();
    }
}
# 2 "./torture/stackalign/test-unwind.h" 2







extern "C" void abort (void);




extern void foo(void);
# 44 "./torture/stackalign/test-unwind.h"
void __attribute__ ((noinline))
copy (char *p, int size)
{
  __builtin_strncpy (p, "good", size);
}

int g_edi __attribute__((externally_visible)) =1;
int g_esi __attribute__((externally_visible)) =2;
int g_ebx __attribute__((externally_visible)) = 3;
int g_ebp __attribute__((externally_visible));
int g_esp __attribute__((externally_visible));
int g_ebp_save __attribute__((externally_visible));
int g_esp_save __attribute__((externally_visible));
int n_error;

int
main()
{
        int dummy;



 __asm__ __volatile__ (
 "movl %1, %0"
 : "=D" (dummy)
 : "i" (1)
 );
 __asm__ __volatile__ (
 "movl %1, %0"
 : "=S" (dummy)
 : "i" (2)
 );
 __asm__ __volatile__ (
 "movl %1, %0"
 : "=b" (dummy)
 : "i" (3)
 );
 __asm__ __volatile__ (
 "movl %ebp," "" "g_ebp_save""\n\t"
 "movl %esp," "" "g_esp_save""\n\t"
 );
 try {
  foo();
 }
 catch (...)
 {
 }


 __asm__ __volatile__ (
 "movl %edi," "" "g_edi""\n\t"
 "movl %esi," "" "g_esi""\n\t"
 "movl %ebx," "" "g_ebx""\n\t"
 "movl %ebp," "" "g_ebp""\n\t"
 "movl %esp," "" "g_esp""\n\t"
 );



 if (g_edi != 1)
 {
  n_error++;



 }
 if (g_esi != 2)
 {
  n_error++;



 }
 if (g_ebx != 3)
 {
  n_error++;



 }
 if (g_ebp != g_ebp_save)
 {
  n_error++;



 }
 if (g_esp != g_esp_save)
 {
  n_error++;



 }
 if (n_error !=0)
  abort();
 return 0;
}
# 4 "./torture/stackalign/unwind-3.C" 2



void __attribute__ ((noinline)) __attribute__ ((regparm(3)))
bar (int arg1, int arg2, int arg3)
{
  int __attribute__ ((aligned(64))) a=1;
  char * s = (char *) __builtin_alloca (arg3 + 1);

  copy (s, arg3);
  if (__builtin_strncmp (s, "good", arg3) != 0)
    {




      abort ();
    }

  if (check_int (&a, __alignof__(a)) != a)
    abort ();

  { int dummy; __asm__ __volatile__ ( "movl %1, %0" : "=D" (dummy) : "i" (-1) ); __asm__ __volatile__ ( "movl %1, %0" : "=S" (dummy) : "i" (-2) ); __asm__ __volatile__ ( "movl %1, %0" : "=b" (dummy) : "i" (-3) ); };
  throw arg1+arg2+arg3+a;
}

void
foo()
{
  bar (1,2,3);
}
