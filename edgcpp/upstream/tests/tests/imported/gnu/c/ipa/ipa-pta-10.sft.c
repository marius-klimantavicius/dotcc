//type: rp
//options: 
# 0 "./ipa/ipa-pta-10.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ipa/ipa-pta-10.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./ipa/ipa-pta-10.c" 2


# 6 "./ipa/ipa-pta-10.c"
static void __attribute__((noinline,noclone))
foo (int i, ...)
{
  va_list ap;
  int *p;
  
# 11 "./ipa/ipa-pta-10.c" 3 4
 __builtin_c23_va_start(
# 11 "./ipa/ipa-pta-10.c"
 ap, i
# 11 "./ipa/ipa-pta-10.c" 3 4
 )
# 11 "./ipa/ipa-pta-10.c"
                 ;
  p = 
# 12 "./ipa/ipa-pta-10.c" 3 4
     __builtin_va_arg(
# 12 "./ipa/ipa-pta-10.c"
     ap
# 12 "./ipa/ipa-pta-10.c" 3 4
     ,
# 12 "./ipa/ipa-pta-10.c"
     int *
# 12 "./ipa/ipa-pta-10.c" 3 4
     )
# 12 "./ipa/ipa-pta-10.c"
                       ;
  *p = 1;
  
# 14 "./ipa/ipa-pta-10.c" 3 4
 __builtin_va_end(
# 14 "./ipa/ipa-pta-10.c"
 ap
# 14 "./ipa/ipa-pta-10.c" 3 4
 )
# 14 "./ipa/ipa-pta-10.c"
            ;
}
extern void abort (void);
int main()
{
  int i = 0;
  foo (0, &i);
  if (i != 1)
    abort ();
  return 0;
}
