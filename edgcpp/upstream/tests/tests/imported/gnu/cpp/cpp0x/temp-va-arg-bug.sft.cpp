//type: fp
//options: --c++11
# 0 "./cpp0x/temp-va-arg-bug.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp0x/temp-va-arg-bug.C"


# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 4 "./cpp0x/temp-va-arg-bug.C" 2


# 5 "./cpp0x/temp-va-arg-bug.C"
struct S { };
void f(S const &);

void g(va_list args)
{
  f(
# 10 "./cpp0x/temp-va-arg-bug.C" 3 4
   __builtin_va_arg(
# 10 "./cpp0x/temp-va-arg-bug.C"
   args
# 10 "./cpp0x/temp-va-arg-bug.C" 3 4
   ,
# 10 "./cpp0x/temp-va-arg-bug.C"
   S
# 10 "./cpp0x/temp-va-arg-bug.C" 3 4
   )
# 10 "./cpp0x/temp-va-arg-bug.C"
                  );
}
