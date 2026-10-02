//type: fp
//options: 
# 0 "./pr46130-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr46130-1.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./pr46130-1.c" 2


# 7 "./pr46130-1.c"
static void
foo (va_list ap)
{
  
# 10 "./pr46130-1.c" 3 4
 __builtin_va_arg(
# 10 "./pr46130-1.c"
 ap
# 10 "./pr46130-1.c" 3 4
 ,
# 10 "./pr46130-1.c"
 char *
# 10 "./pr46130-1.c" 3 4
 )
# 10 "./pr46130-1.c"
                    [0];
}

void
bar (va_list ap)
{
  foo (ap);
}

void
baz (va_list ap)
{
  foo (ap);
}
