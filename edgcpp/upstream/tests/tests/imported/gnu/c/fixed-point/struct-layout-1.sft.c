//type: rp
//options: --c99 --strict_gnu
# 0 "./fixed-point/struct-layout-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./fixed-point/struct-layout-1.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 7 "./fixed-point/struct-layout-1.c" 2


# 8 "./fixed-point/struct-layout-1.c"
extern void abort (void);

struct S1
{
  long _Fract a[0];
};

struct S2
{
  struct
  {
    long _Fract e;
  } b[0];
};

struct S3
{
  union
  {
    long _Fract c;
  } a[0];
};

struct S4
{
  int a[0];
  long _Fract b[0];
};

struct S5
{
  union
  {
    long _Fract c[0];
  } a;
};

int check_var (int z, ...)
{
  long long result;
  va_list ap;
  
# 49 "./fixed-point/struct-layout-1.c" 3 4
 __builtin_va_start(
# 49 "./fixed-point/struct-layout-1.c"
 ap
# 49 "./fixed-point/struct-layout-1.c" 3 4
 ,
# 49 "./fixed-point/struct-layout-1.c"
 z
# 49 "./fixed-point/struct-layout-1.c" 3 4
 )
# 49 "./fixed-point/struct-layout-1.c"
                 ;
  
# 50 "./fixed-point/struct-layout-1.c" 3 4
 __builtin_va_arg(
# 50 "./fixed-point/struct-layout-1.c"
 ap
# 50 "./fixed-point/struct-layout-1.c" 3 4
 ,
# 50 "./fixed-point/struct-layout-1.c"
 struct S1
# 50 "./fixed-point/struct-layout-1.c" 3 4
 )
# 50 "./fixed-point/struct-layout-1.c"
                       ;
  result = 
# 51 "./fixed-point/struct-layout-1.c" 3 4
          __builtin_va_arg(
# 51 "./fixed-point/struct-layout-1.c"
          ap
# 51 "./fixed-point/struct-layout-1.c" 3 4
          ,
# 51 "./fixed-point/struct-layout-1.c"
          long long
# 51 "./fixed-point/struct-layout-1.c" 3 4
          )
# 51 "./fixed-point/struct-layout-1.c"
                                ;
  
# 52 "./fixed-point/struct-layout-1.c" 3 4
 __builtin_va_end(
# 52 "./fixed-point/struct-layout-1.c"
 ap
# 52 "./fixed-point/struct-layout-1.c" 3 4
 )
# 52 "./fixed-point/struct-layout-1.c"
            ;

  return (result == 2LL);
}

int main ()
{
  struct S1 s1;
  struct S2 s2;
  struct S3 s3;
  struct S4 s4;
  struct S5 s5;

  if (check_var (2, s1, 2LL) == 0)
    abort ();
  if (check_var (2, s2, 2LL) == 0)
    abort ();
  if (check_var (2, s3, 2LL) == 0)
    abort ();
  if (check_var (2, s4, 2LL) == 0)
    abort ();
  if (check_var (2, s5, 2LL) == 0)
    abort ();

  return 0;
}
