//type: fp
//options: 
# 0 "./torture/pr48731.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr48731.c"


# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 4 "./torture/pr48731.c" 2


# 5 "./torture/pr48731.c"
int blah(int a, ...)
{
  va_list va;
  
# 8 "./torture/pr48731.c" 3 4
 __builtin_c23_va_start(
# 8 "./torture/pr48731.c"
 va,a
# 8 "./torture/pr48731.c" 3 4
 )
# 8 "./torture/pr48731.c"
               ;
  if (a == 0)
    return -1;
  else
    {
      int i;
      for (i = 0; i < a; i++)
 
# 15 "./torture/pr48731.c" 3 4
__builtin_va_arg(
# 15 "./torture/pr48731.c"
va
# 15 "./torture/pr48731.c" 3 4
,
# 15 "./torture/pr48731.c"
int
# 15 "./torture/pr48731.c" 3 4
)
# 15 "./torture/pr48731.c"
              ;
      return 
# 16 "./torture/pr48731.c" 3 4
            __builtin_va_arg(
# 16 "./torture/pr48731.c"
            va
# 16 "./torture/pr48731.c" 3 4
            ,
# 16 "./torture/pr48731.c"
            int
# 16 "./torture/pr48731.c" 3 4
            )
# 16 "./torture/pr48731.c"
                          ;
    }
}

__attribute((flatten))
int blah2(int b, int c)
{
  return blah(2, b, c);
}
