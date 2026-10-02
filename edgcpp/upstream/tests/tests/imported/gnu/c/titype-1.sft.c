//type: rp
//options: 
# 0 "./titype-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./titype-1.c"




typedef int TItype __attribute__ ((mode (TI)));




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 11 "./titype-1.c" 2


# 12 "./titype-1.c"
extern void abort(void);


void foo(int i, ...)
{
  TItype q;
  va_list va;

  
# 20 "./titype-1.c" 3 4
 __builtin_c23_va_start(
# 20 "./titype-1.c"
 va, i
# 20 "./titype-1.c" 3 4
 )
# 20 "./titype-1.c"
                ;
  q = 
# 21 "./titype-1.c" 3 4
     __builtin_va_arg(
# 21 "./titype-1.c"
     va
# 21 "./titype-1.c" 3 4
     ,
# 21 "./titype-1.c"
     TItype
# 21 "./titype-1.c" 3 4
     )
# 21 "./titype-1.c"
                       ;
  
# 22 "./titype-1.c" 3 4
 __builtin_va_end(
# 22 "./titype-1.c"
 va
# 22 "./titype-1.c" 3 4
 )
# 22 "./titype-1.c"
           ;

  if (q != 5)
    abort();
}

int main(void)
{
  TItype q = 5;

  foo(1, q);
  return 0;
}
