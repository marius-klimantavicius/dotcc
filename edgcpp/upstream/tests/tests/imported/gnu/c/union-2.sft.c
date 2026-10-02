//type: rp
//options: 
# 0 "./union-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./union-2.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 7 "./union-2.c" 2


# 8 "./union-2.c"
union U
{
  long l1[2];
};

union U u;

void foo (int z, ...)
{
  int i;
  va_list ap;
  
# 19 "./union-2.c" 3 4
 __builtin_c23_va_start(
# 19 "./union-2.c"
 ap,z
# 19 "./union-2.c" 3 4
 )
# 19 "./union-2.c"
               ;
  i = 
# 20 "./union-2.c" 3 4
     __builtin_va_arg(
# 20 "./union-2.c"
     ap
# 20 "./union-2.c" 3 4
     ,
# 20 "./union-2.c"
     int
# 20 "./union-2.c" 3 4
     )
# 20 "./union-2.c"
                    ;
  
# 21 "./union-2.c" 3 4
 __builtin_va_end(
# 21 "./union-2.c"
 ap
# 21 "./union-2.c" 3 4
 )
# 21 "./union-2.c"
           ;
}

int main(void)
{
  foo (1, 1, 1, 1, 1, u);
  return 0;
}
