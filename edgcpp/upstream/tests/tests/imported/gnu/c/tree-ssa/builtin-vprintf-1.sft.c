//type: fp
//options: 
# 0 "./tree-ssa/builtin-vprintf-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/builtin-vprintf-1.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./tree-ssa/builtin-vprintf-1.c" 2


# 6 "./tree-ssa/builtin-vprintf-1.c"
extern int vprintf (const char *, va_list);
volatile int vi0, vi1, vi2, vi3, vi4, vi5, vi6, vi7, vi8, vi9, via;

void
test (va_list ap1, va_list ap2, va_list ap3, va_list ap4, va_list ap5,
      va_list ap6, va_list ap7)
{
  vi0 = 0;
  vprintf ("hello", ap1);
  vi1 = 0;
  vprintf ("hello\n", ap2);
  vi2 = 0;
  vprintf ("a", ap3);
  vi3 = 0;
  vprintf ("", ap4);
  vi4 = 0;
  vprintf ("%s", ap5);
  vi5 = 0;
  vprintf ("%c", ap6);
  vi6 = 0;
  vprintf ("%s\n", ap7);
  vi7 = 0;
}
