//type: rp
//options: --c99 --strict_gnu
# 0 "./fixed-point/func-vararg-size0.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./fixed-point/func-vararg-size0.c"






# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 8 "./fixed-point/func-vararg-size0.c" 2


# 9 "./fixed-point/func-vararg-size0.c"
extern void abort (void);

struct S1
{
  struct
  {
    _Fract e;
  } b[0];
};



int check_var(int z,...)
{
  double d;
  struct S1 s1;
  long long result;
  va_list ap;
  
# 27 "./fixed-point/func-vararg-size0.c" 3 4
 __builtin_va_start(
# 27 "./fixed-point/func-vararg-size0.c"
 ap
# 27 "./fixed-point/func-vararg-size0.c" 3 4
 ,
# 27 "./fixed-point/func-vararg-size0.c"
 z
# 27 "./fixed-point/func-vararg-size0.c" 3 4
 )
# 27 "./fixed-point/func-vararg-size0.c"
                 ;
  d = 
# 28 "./fixed-point/func-vararg-size0.c" 3 4
     __builtin_va_arg(
# 28 "./fixed-point/func-vararg-size0.c"
     ap
# 28 "./fixed-point/func-vararg-size0.c" 3 4
     ,
# 28 "./fixed-point/func-vararg-size0.c"
     double
# 28 "./fixed-point/func-vararg-size0.c" 3 4
     )
# 28 "./fixed-point/func-vararg-size0.c"
                        ;
  s1 = 
# 29 "./fixed-point/func-vararg-size0.c" 3 4
      __builtin_va_arg(
# 29 "./fixed-point/func-vararg-size0.c"
      ap
# 29 "./fixed-point/func-vararg-size0.c" 3 4
      ,
# 29 "./fixed-point/func-vararg-size0.c"
      struct S1
# 29 "./fixed-point/func-vararg-size0.c" 3 4
      )
# 29 "./fixed-point/func-vararg-size0.c"
                            ;
  result = 
# 30 "./fixed-point/func-vararg-size0.c" 3 4
          __builtin_va_arg(
# 30 "./fixed-point/func-vararg-size0.c"
          ap
# 30 "./fixed-point/func-vararg-size0.c" 3 4
          ,
# 30 "./fixed-point/func-vararg-size0.c"
          long long
# 30 "./fixed-point/func-vararg-size0.c" 3 4
          )
# 30 "./fixed-point/func-vararg-size0.c"
                                ;
  
# 31 "./fixed-point/func-vararg-size0.c" 3 4
 __builtin_va_end(
# 31 "./fixed-point/func-vararg-size0.c"
 ap
# 31 "./fixed-point/func-vararg-size0.c" 3 4
 )
# 31 "./fixed-point/func-vararg-size0.c"
            ;
  return (result == 2LL);

}

int
main ()
{
  struct S1 s1;
  struct S1 a1[5];

  if (check_var(5, 1.0, s1, 2LL, a1[2], a1[2]) == 0)
    abort ();

  return 0;
}
