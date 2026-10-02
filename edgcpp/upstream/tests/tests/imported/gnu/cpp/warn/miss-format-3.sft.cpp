//type: fp
//options: 
# 0 "./warn/miss-format-3.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/miss-format-3.C"






# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 8 "./warn/miss-format-3.C" 2


# 9 "./warn/miss-format-3.C"
typedef void (*noattr_t) (const char *, ...);
typedef noattr_t __attribute__ ((__format__(__printf__, 1, 2))) attr_t;

typedef void (*vnoattr_t) (const char *, va_list);
typedef vnoattr_t __attribute__ ((__format__(__printf__, 1, 0))) vattr_t;

void
foo1 (noattr_t na, attr_t a, vnoattr_t vna, vattr_t va)
{
  noattr_t na1 = na;
  noattr_t na2 = a;
  attr_t a1 = na;
  attr_t a2 = a;

  vnoattr_t vna1 = vna;
  vnoattr_t vna2 = va;
  vattr_t va1 = vna;
  vattr_t va2 = va;
}
