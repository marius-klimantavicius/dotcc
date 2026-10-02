//type: fp
//options: 
# 0 "./tree-ssa/stdarg-6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/stdarg-6.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./tree-ssa/stdarg-6.c" 2


# 7 "./tree-ssa/stdarg-6.c"
int a, b;
char c[128];

static inline void
foo (int x, char const *y, va_list z)
{
  __builtin_printf ("%s %d %s", x ? "" : "foo", ++a, (y && *y) ? "bar" : "");
  if (y && *y)
    __builtin_vprintf (y, z);
}

void
bar (int x, char const *y, ...)
{
  va_list z;
  
# 22 "./tree-ssa/stdarg-6.c" 3 4
 __builtin_c23_va_start(
# 22 "./tree-ssa/stdarg-6.c"
 z, y
# 22 "./tree-ssa/stdarg-6.c" 3 4
 )
# 22 "./tree-ssa/stdarg-6.c"
                ;
  if (!x && *c == '\0')
    ++b;
  foo (x, y, z);
  
# 26 "./tree-ssa/stdarg-6.c" 3 4
 __builtin_va_end(
# 26 "./tree-ssa/stdarg-6.c"
 z
# 26 "./tree-ssa/stdarg-6.c" 3 4
 )
# 26 "./tree-ssa/stdarg-6.c"
           ;
}
