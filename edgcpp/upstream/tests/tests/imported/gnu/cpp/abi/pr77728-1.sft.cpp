//type: fp
//options: 
# 0 "./abi/pr77728-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/pr77728-1.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./abi/pr77728-1.C" 2


# 6 "./abi/pr77728-1.C"
template <int N>
struct A { double p; };

A<0> v;

template <int N>
struct B
{
  typedef A<N> T;
  int i, j;
};

struct C : public B<0> {};
struct D {};
struct E : public D, C {};
struct F : public B<1> {};
struct G : public F { static double y; };
struct H : public G {};
struct I : public D { long long z; };
struct J : public D { static double z; int i, j; };

template <int N>
struct K : public D { typedef A<N> T; int i, j; };

struct L { static double h; int i, j; };

int
fn1 (int a, B<0> b)
{
  return a + b.i;
}

int
fn2 (int a, B<1> b)
{
  return a + b.i;
}

int
fn3 (int a, L b)
{
  return a + b.i;
}

int
fn4 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, B<0> n, ...)

{
  va_list ap;
  
# 55 "./abi/pr77728-1.C" 3 4
 __builtin_va_start(
# 55 "./abi/pr77728-1.C"
 ap
# 55 "./abi/pr77728-1.C" 3 4
 ,
# 55 "./abi/pr77728-1.C"
 n
# 55 "./abi/pr77728-1.C" 3 4
 )
# 55 "./abi/pr77728-1.C"
                 ;
  int x = 
# 56 "./abi/pr77728-1.C" 3 4
         __builtin_va_arg(
# 56 "./abi/pr77728-1.C"
         ap
# 56 "./abi/pr77728-1.C" 3 4
         ,
# 56 "./abi/pr77728-1.C"
         int
# 56 "./abi/pr77728-1.C" 3 4
         )
# 56 "./abi/pr77728-1.C"
                         ;
  
# 57 "./abi/pr77728-1.C" 3 4
 __builtin_va_end(
# 57 "./abi/pr77728-1.C"
 ap
# 57 "./abi/pr77728-1.C" 3 4
 )
# 57 "./abi/pr77728-1.C"
            ;
  return x;
}

int
fn5 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, B<1> n, ...)
{
  va_list ap;
  
# 65 "./abi/pr77728-1.C" 3 4
 __builtin_va_start(
# 65 "./abi/pr77728-1.C"
 ap
# 65 "./abi/pr77728-1.C" 3 4
 ,
# 65 "./abi/pr77728-1.C"
 n
# 65 "./abi/pr77728-1.C" 3 4
 )
# 65 "./abi/pr77728-1.C"
                 ;
  int x = 
# 66 "./abi/pr77728-1.C" 3 4
         __builtin_va_arg(
# 66 "./abi/pr77728-1.C"
         ap
# 66 "./abi/pr77728-1.C" 3 4
         ,
# 66 "./abi/pr77728-1.C"
         int
# 66 "./abi/pr77728-1.C" 3 4
         )
# 66 "./abi/pr77728-1.C"
                         ;
  
# 67 "./abi/pr77728-1.C" 3 4
 __builtin_va_end(
# 67 "./abi/pr77728-1.C"
 ap
# 67 "./abi/pr77728-1.C" 3 4
 )
# 67 "./abi/pr77728-1.C"
            ;
  return x;
}

int
fn6 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, C n, ...)
{
  va_list ap;
  
# 75 "./abi/pr77728-1.C" 3 4
 __builtin_va_start(
# 75 "./abi/pr77728-1.C"
 ap
# 75 "./abi/pr77728-1.C" 3 4
 ,
# 75 "./abi/pr77728-1.C"
 n
# 75 "./abi/pr77728-1.C" 3 4
 )
# 75 "./abi/pr77728-1.C"
                 ;
  int x = 
# 76 "./abi/pr77728-1.C" 3 4
         __builtin_va_arg(
# 76 "./abi/pr77728-1.C"
         ap
# 76 "./abi/pr77728-1.C" 3 4
         ,
# 76 "./abi/pr77728-1.C"
         int
# 76 "./abi/pr77728-1.C" 3 4
         )
# 76 "./abi/pr77728-1.C"
                         ;
  
# 77 "./abi/pr77728-1.C" 3 4
 __builtin_va_end(
# 77 "./abi/pr77728-1.C"
 ap
# 77 "./abi/pr77728-1.C" 3 4
 )
# 77 "./abi/pr77728-1.C"
            ;
  return x;
}

int
fn7 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, E n, ...)
{
  va_list ap;
  
# 85 "./abi/pr77728-1.C" 3 4
 __builtin_va_start(
# 85 "./abi/pr77728-1.C"
 ap
# 85 "./abi/pr77728-1.C" 3 4
 ,
# 85 "./abi/pr77728-1.C"
 n
# 85 "./abi/pr77728-1.C" 3 4
 )
# 85 "./abi/pr77728-1.C"
                 ;
  int x = 
# 86 "./abi/pr77728-1.C" 3 4
         __builtin_va_arg(
# 86 "./abi/pr77728-1.C"
         ap
# 86 "./abi/pr77728-1.C" 3 4
         ,
# 86 "./abi/pr77728-1.C"
         int
# 86 "./abi/pr77728-1.C" 3 4
         )
# 86 "./abi/pr77728-1.C"
                         ;
  
# 87 "./abi/pr77728-1.C" 3 4
 __builtin_va_end(
# 87 "./abi/pr77728-1.C"
 ap
# 87 "./abi/pr77728-1.C" 3 4
 )
# 87 "./abi/pr77728-1.C"
            ;
  return x;
}

int
fn8 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, H n, ...)
{
  va_list ap;
  
# 95 "./abi/pr77728-1.C" 3 4
 __builtin_va_start(
# 95 "./abi/pr77728-1.C"
 ap
# 95 "./abi/pr77728-1.C" 3 4
 ,
# 95 "./abi/pr77728-1.C"
 n
# 95 "./abi/pr77728-1.C" 3 4
 )
# 95 "./abi/pr77728-1.C"
                 ;
  int x = 
# 96 "./abi/pr77728-1.C" 3 4
         __builtin_va_arg(
# 96 "./abi/pr77728-1.C"
         ap
# 96 "./abi/pr77728-1.C" 3 4
         ,
# 96 "./abi/pr77728-1.C"
         int
# 96 "./abi/pr77728-1.C" 3 4
         )
# 96 "./abi/pr77728-1.C"
                         ;
  
# 97 "./abi/pr77728-1.C" 3 4
 __builtin_va_end(
# 97 "./abi/pr77728-1.C"
 ap
# 97 "./abi/pr77728-1.C" 3 4
 )
# 97 "./abi/pr77728-1.C"
            ;
  return x;
}

int
fn9 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, I n, ...)
{
  va_list ap;
  
# 105 "./abi/pr77728-1.C" 3 4
 __builtin_va_start(
# 105 "./abi/pr77728-1.C"
 ap
# 105 "./abi/pr77728-1.C" 3 4
 ,
# 105 "./abi/pr77728-1.C"
 n
# 105 "./abi/pr77728-1.C" 3 4
 )
# 105 "./abi/pr77728-1.C"
                 ;
  int x = 
# 106 "./abi/pr77728-1.C" 3 4
         __builtin_va_arg(
# 106 "./abi/pr77728-1.C"
         ap
# 106 "./abi/pr77728-1.C" 3 4
         ,
# 106 "./abi/pr77728-1.C"
         int
# 106 "./abi/pr77728-1.C" 3 4
         )
# 106 "./abi/pr77728-1.C"
                         ;
  
# 107 "./abi/pr77728-1.C" 3 4
 __builtin_va_end(
# 107 "./abi/pr77728-1.C"
 ap
# 107 "./abi/pr77728-1.C" 3 4
 )
# 107 "./abi/pr77728-1.C"
            ;
  return x;
}

int
fn10 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, J n, ...)

{
  va_list ap;
  
# 116 "./abi/pr77728-1.C" 3 4
 __builtin_va_start(
# 116 "./abi/pr77728-1.C"
 ap
# 116 "./abi/pr77728-1.C" 3 4
 ,
# 116 "./abi/pr77728-1.C"
 n
# 116 "./abi/pr77728-1.C" 3 4
 )
# 116 "./abi/pr77728-1.C"
                 ;
  int x = 
# 117 "./abi/pr77728-1.C" 3 4
         __builtin_va_arg(
# 117 "./abi/pr77728-1.C"
         ap
# 117 "./abi/pr77728-1.C" 3 4
         ,
# 117 "./abi/pr77728-1.C"
         int
# 117 "./abi/pr77728-1.C" 3 4
         )
# 117 "./abi/pr77728-1.C"
                         ;
  
# 118 "./abi/pr77728-1.C" 3 4
 __builtin_va_end(
# 118 "./abi/pr77728-1.C"
 ap
# 118 "./abi/pr77728-1.C" 3 4
 )
# 118 "./abi/pr77728-1.C"
            ;
  return x;
}

int
fn11 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, K<0> n, ...)

{
  va_list ap;
  
# 127 "./abi/pr77728-1.C" 3 4
 __builtin_va_start(
# 127 "./abi/pr77728-1.C"
 ap
# 127 "./abi/pr77728-1.C" 3 4
 ,
# 127 "./abi/pr77728-1.C"
 n
# 127 "./abi/pr77728-1.C" 3 4
 )
# 127 "./abi/pr77728-1.C"
                 ;
  int x = 
# 128 "./abi/pr77728-1.C" 3 4
         __builtin_va_arg(
# 128 "./abi/pr77728-1.C"
         ap
# 128 "./abi/pr77728-1.C" 3 4
         ,
# 128 "./abi/pr77728-1.C"
         int
# 128 "./abi/pr77728-1.C" 3 4
         )
# 128 "./abi/pr77728-1.C"
                         ;
  
# 129 "./abi/pr77728-1.C" 3 4
 __builtin_va_end(
# 129 "./abi/pr77728-1.C"
 ap
# 129 "./abi/pr77728-1.C" 3 4
 )
# 129 "./abi/pr77728-1.C"
            ;
  return x;
}

int
fn12 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, K<2> n, ...)
{
  va_list ap;
  
# 137 "./abi/pr77728-1.C" 3 4
 __builtin_va_start(
# 137 "./abi/pr77728-1.C"
 ap
# 137 "./abi/pr77728-1.C" 3 4
 ,
# 137 "./abi/pr77728-1.C"
 n
# 137 "./abi/pr77728-1.C" 3 4
 )
# 137 "./abi/pr77728-1.C"
                 ;
  int x = 
# 138 "./abi/pr77728-1.C" 3 4
         __builtin_va_arg(
# 138 "./abi/pr77728-1.C"
         ap
# 138 "./abi/pr77728-1.C" 3 4
         ,
# 138 "./abi/pr77728-1.C"
         int
# 138 "./abi/pr77728-1.C" 3 4
         )
# 138 "./abi/pr77728-1.C"
                         ;
  
# 139 "./abi/pr77728-1.C" 3 4
 __builtin_va_end(
# 139 "./abi/pr77728-1.C"
 ap
# 139 "./abi/pr77728-1.C" 3 4
 )
# 139 "./abi/pr77728-1.C"
            ;
  return x;
}

void
test ()
{
  static B<0> b0;
  static B<1> b1;
  static L l;
  static C c;
  static E e;
  static H h;
  static I i;
  static J j;
  static K<0> k0;
  static K<2> k2;
  fn1 (1, b0);
  fn2 (1, b1);
  fn3 (1, l);
  fn4 (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, b0, 1, 2, 3, 4);

  fn5 (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, b1, 1, 2, 3, 4);
  fn6 (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, c, 1, 2, 3, 4);
  fn7 (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, e, 1, 2, 3, 4);
  fn8 (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, h, 1, 2, 3, 4);
  fn9 (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, i, 1, 2, 3, 4);
  fn10 (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, j, 1, 2, 3, 4);

  fn11 (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, k0, 1, 2, 3, 4);

  fn12 (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, k2, 1, 2, 3, 4);
}
