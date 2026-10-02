//type: fp
//options: 
# 0 "./asan/pr83185.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./asan/pr83185.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./asan/pr83185.c" 2


# 6 "./asan/pr83185.c"
int bar (void);

void
foo (int i, ...)
{
  va_list aps[bar()];
  
# 12 "./asan/pr83185.c" 3 4
 __builtin_c23_va_start(
# 12 "./asan/pr83185.c"
 aps[4], i
# 12 "./asan/pr83185.c" 3 4
 )
# 12 "./asan/pr83185.c"
                     ;
  
# 13 "./asan/pr83185.c" 3 4
 __builtin_va_end(
# 13 "./asan/pr83185.c"
 aps[4]
# 13 "./asan/pr83185.c" 3 4
 )
# 13 "./asan/pr83185.c"
                ;
}
