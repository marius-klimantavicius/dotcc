//type: fp
//options: 
# 0 "./pr101170.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr101170.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./pr101170.c" 2


# 7 "./pr101170.c"
struct S { int a; int b[4]; } s;
va_list ap;
int i;
long long l;

struct S
foo (int x)
{
  struct S a = {};
  do
    if (x)
      return a;
  while (1);
}

__attribute__((noipa)) void
bar (void)
{
  for (; i; i++)
    l |= 
# 26 "./pr101170.c" 3 4
        __builtin_va_arg(
# 26 "./pr101170.c"
        ap
# 26 "./pr101170.c" 3 4
        ,
# 26 "./pr101170.c"
        long long
# 26 "./pr101170.c" 3 4
        ) 
# 26 "./pr101170.c"
                               << s.b[i];
  if (l)
    foo (l);
}

void
baz (int v, ...)
{
  
# 34 "./pr101170.c" 3 4
 __builtin_c23_va_start(
# 34 "./pr101170.c"
 ap, v
# 34 "./pr101170.c" 3 4
 )
# 34 "./pr101170.c"
                 ;
  bar ();
  
# 36 "./pr101170.c" 3 4
 __builtin_va_end(
# 36 "./pr101170.c"
 ap
# 36 "./pr101170.c" 3 4
 )
# 36 "./pr101170.c"
            ;
}
