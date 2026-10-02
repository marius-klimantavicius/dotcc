//type: fp
//options: 
# 0 "./expr/stdarg2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./expr/stdarg2.C"


# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 4 "./expr/stdarg2.C" 2


# 5 "./expr/stdarg2.C"
struct S
{
  double a;
};

void
foo (int z, ...)
{
  struct S arg;
  va_list ap;
  arg = 
# 15 "./expr/stdarg2.C" 3 4
       __builtin_va_arg(
# 15 "./expr/stdarg2.C"
       ap
# 15 "./expr/stdarg2.C" 3 4
       ,
# 15 "./expr/stdarg2.C"
       struct S
# 15 "./expr/stdarg2.C" 3 4
       )
# 15 "./expr/stdarg2.C"
                            ;
}


struct T
{
  __complex__ float a;
};

void
bar (int z, ...)
{
  struct T arg;
  va_list ap;
  arg = 
# 29 "./expr/stdarg2.C" 3 4
       __builtin_va_arg(
# 29 "./expr/stdarg2.C"
       ap
# 29 "./expr/stdarg2.C" 3 4
       ,
# 29 "./expr/stdarg2.C"
       struct T
# 29 "./expr/stdarg2.C" 3 4
       )
# 29 "./expr/stdarg2.C"
                            ;
}
