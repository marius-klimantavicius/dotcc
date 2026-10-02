//type: fp
//options:  --c++20 --modules
# 0 "./modules/builtin-3_a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/builtin-3_a.C"

module;
# 1 "/mds/gnu/build/gcc-15.1.0/lib/gcc/x86_64-pc-linux-gnu/15.1.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15.1.0/lib/gcc/x86_64-pc-linux-gnu/15.1.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15.1.0/lib/gcc/x86_64-pc-linux-gnu/15.1.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15.1.0/lib/gcc/x86_64-pc-linux-gnu/15.1.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 4 "./modules/builtin-3_a.C" 2

# 4 "./modules/builtin-3_a.C"
export module builtins;


export inline unsigned length (char const *ptr)
{
  return __builtin_strlen (ptr);
}

export inline int count (int a, ...)
{
  int c = 0;

  va_list args;
  
# 17 "./modules/builtin-3_a.C" 3 4
 __builtin_va_start(
# 17 "./modules/builtin-3_a.C"
 args
# 17 "./modules/builtin-3_a.C" 3 4
 ,
# 17 "./modules/builtin-3_a.C"
 a
# 17 "./modules/builtin-3_a.C" 3 4
 )
# 17 "./modules/builtin-3_a.C"
                   ;
  while (
# 18 "./modules/builtin-3_a.C" 3 4
        __builtin_va_arg(
# 18 "./modules/builtin-3_a.C"
        args
# 18 "./modules/builtin-3_a.C" 3 4
        ,
# 18 "./modules/builtin-3_a.C"
        char *
# 18 "./modules/builtin-3_a.C" 3 4
        )
# 18 "./modules/builtin-3_a.C"
                             )
    c++;
  
# 20 "./modules/builtin-3_a.C" 3 4
 __builtin_va_end(
# 20 "./modules/builtin-3_a.C"
 args
# 20 "./modules/builtin-3_a.C" 3 4
 )
# 20 "./modules/builtin-3_a.C"
              ;

  return c;
}
