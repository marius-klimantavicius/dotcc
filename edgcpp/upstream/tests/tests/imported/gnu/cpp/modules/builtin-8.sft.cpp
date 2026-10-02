//type: fp
//options:  --c++20 --modules
# 0 "./modules/builtin-8.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/builtin-8.C"

module;
# 1 "/mds/gnu/build/gcc-15.1.0/lib/gcc/x86_64-pc-linux-gnu/15.1.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15.1.0/lib/gcc/x86_64-pc-linux-gnu/15.1.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15.1.0/lib/gcc/x86_64-pc-linux-gnu/15.1.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15.1.0/lib/gcc/x86_64-pc-linux-gnu/15.1.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 4 "./modules/builtin-8.C" 2

# 4 "./modules/builtin-8.C"
export module builtins;


export {
  using ::va_list;
}
