//type: fp
//options: --c++11
# 0 "./abi/pr77728-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/pr77728-2.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./abi/pr77728-2.C" 2


# 6 "./abi/pr77728-2.C"
template <int N>
struct alignas (16) A { char p[16]; };

A<0> v;

template <int N>
struct B
{
  typedef A<N> T;
  int i, j, k, l;
};

struct C : public B<0> {};
struct D {};
struct E : public D, C {};
struct F : public B<1> {};
struct G : public F { static int y alignas (16); };
struct H : public G {};
struct I : public D { int z alignas (16); };
struct J : public D { static int z alignas (16); int i, j, k, l; };

template <int N>
struct K : public D { typedef A<N> T; int i, j; };

struct L { static int h alignas (16); int i, j, k, l; };

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
  
# 54 "./abi/pr77728-2.C" 3 4
 __builtin_va_start(
# 54 "./abi/pr77728-2.C"
 ap
# 54 "./abi/pr77728-2.C" 3 4
 ,
# 54 "./abi/pr77728-2.C"
 n
# 54 "./abi/pr77728-2.C" 3 4
 )
# 54 "./abi/pr77728-2.C"
                 ;
  int x = 
# 55 "./abi/pr77728-2.C" 3 4
         __builtin_va_arg(
# 55 "./abi/pr77728-2.C"
         ap
# 55 "./abi/pr77728-2.C" 3 4
         ,
# 55 "./abi/pr77728-2.C"
         int
# 55 "./abi/pr77728-2.C" 3 4
         )
# 55 "./abi/pr77728-2.C"
                         ;
  
# 56 "./abi/pr77728-2.C" 3 4
 __builtin_va_end(
# 56 "./abi/pr77728-2.C"
 ap
# 56 "./abi/pr77728-2.C" 3 4
 )
# 56 "./abi/pr77728-2.C"
            ;
  return x;
}

int
fn5 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, B<1> n, ...)
{
  va_list ap;
  
# 64 "./abi/pr77728-2.C" 3 4
 __builtin_va_start(
# 64 "./abi/pr77728-2.C"
 ap
# 64 "./abi/pr77728-2.C" 3 4
 ,
# 64 "./abi/pr77728-2.C"
 n
# 64 "./abi/pr77728-2.C" 3 4
 )
# 64 "./abi/pr77728-2.C"
                 ;
  int x = 
# 65 "./abi/pr77728-2.C" 3 4
         __builtin_va_arg(
# 65 "./abi/pr77728-2.C"
         ap
# 65 "./abi/pr77728-2.C" 3 4
         ,
# 65 "./abi/pr77728-2.C"
         int
# 65 "./abi/pr77728-2.C" 3 4
         )
# 65 "./abi/pr77728-2.C"
                         ;
  
# 66 "./abi/pr77728-2.C" 3 4
 __builtin_va_end(
# 66 "./abi/pr77728-2.C"
 ap
# 66 "./abi/pr77728-2.C" 3 4
 )
# 66 "./abi/pr77728-2.C"
            ;
  return x;
}

int
fn6 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, C n, ...)
{
  va_list ap;
  
# 74 "./abi/pr77728-2.C" 3 4
 __builtin_va_start(
# 74 "./abi/pr77728-2.C"
 ap
# 74 "./abi/pr77728-2.C" 3 4
 ,
# 74 "./abi/pr77728-2.C"
 n
# 74 "./abi/pr77728-2.C" 3 4
 )
# 74 "./abi/pr77728-2.C"
                 ;
  int x = 
# 75 "./abi/pr77728-2.C" 3 4
         __builtin_va_arg(
# 75 "./abi/pr77728-2.C"
         ap
# 75 "./abi/pr77728-2.C" 3 4
         ,
# 75 "./abi/pr77728-2.C"
         int
# 75 "./abi/pr77728-2.C" 3 4
         )
# 75 "./abi/pr77728-2.C"
                         ;
  
# 76 "./abi/pr77728-2.C" 3 4
 __builtin_va_end(
# 76 "./abi/pr77728-2.C"
 ap
# 76 "./abi/pr77728-2.C" 3 4
 )
# 76 "./abi/pr77728-2.C"
            ;
  return x;
}

int
fn7 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, E n, ...)
{
  va_list ap;
  
# 84 "./abi/pr77728-2.C" 3 4
 __builtin_va_start(
# 84 "./abi/pr77728-2.C"
 ap
# 84 "./abi/pr77728-2.C" 3 4
 ,
# 84 "./abi/pr77728-2.C"
 n
# 84 "./abi/pr77728-2.C" 3 4
 )
# 84 "./abi/pr77728-2.C"
                 ;
  int x = 
# 85 "./abi/pr77728-2.C" 3 4
         __builtin_va_arg(
# 85 "./abi/pr77728-2.C"
         ap
# 85 "./abi/pr77728-2.C" 3 4
         ,
# 85 "./abi/pr77728-2.C"
         int
# 85 "./abi/pr77728-2.C" 3 4
         )
# 85 "./abi/pr77728-2.C"
                         ;
  
# 86 "./abi/pr77728-2.C" 3 4
 __builtin_va_end(
# 86 "./abi/pr77728-2.C"
 ap
# 86 "./abi/pr77728-2.C" 3 4
 )
# 86 "./abi/pr77728-2.C"
            ;
  return x;
}

int
fn8 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, H n, ...)
{
  va_list ap;
  
# 94 "./abi/pr77728-2.C" 3 4
 __builtin_va_start(
# 94 "./abi/pr77728-2.C"
 ap
# 94 "./abi/pr77728-2.C" 3 4
 ,
# 94 "./abi/pr77728-2.C"
 n
# 94 "./abi/pr77728-2.C" 3 4
 )
# 94 "./abi/pr77728-2.C"
                 ;
  int x = 
# 95 "./abi/pr77728-2.C" 3 4
         __builtin_va_arg(
# 95 "./abi/pr77728-2.C"
         ap
# 95 "./abi/pr77728-2.C" 3 4
         ,
# 95 "./abi/pr77728-2.C"
         int
# 95 "./abi/pr77728-2.C" 3 4
         )
# 95 "./abi/pr77728-2.C"
                         ;
  
# 96 "./abi/pr77728-2.C" 3 4
 __builtin_va_end(
# 96 "./abi/pr77728-2.C"
 ap
# 96 "./abi/pr77728-2.C" 3 4
 )
# 96 "./abi/pr77728-2.C"
            ;
  return x;
}

int
fn9 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, I n, ...)
{
  va_list ap;
  
# 104 "./abi/pr77728-2.C" 3 4
 __builtin_va_start(
# 104 "./abi/pr77728-2.C"
 ap
# 104 "./abi/pr77728-2.C" 3 4
 ,
# 104 "./abi/pr77728-2.C"
 n
# 104 "./abi/pr77728-2.C" 3 4
 )
# 104 "./abi/pr77728-2.C"
                 ;
  int x = 
# 105 "./abi/pr77728-2.C" 3 4
         __builtin_va_arg(
# 105 "./abi/pr77728-2.C"
         ap
# 105 "./abi/pr77728-2.C" 3 4
         ,
# 105 "./abi/pr77728-2.C"
         int
# 105 "./abi/pr77728-2.C" 3 4
         )
# 105 "./abi/pr77728-2.C"
                         ;
  
# 106 "./abi/pr77728-2.C" 3 4
 __builtin_va_end(
# 106 "./abi/pr77728-2.C"
 ap
# 106 "./abi/pr77728-2.C" 3 4
 )
# 106 "./abi/pr77728-2.C"
            ;
  return x;
}

int
fn10 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, J n, ...)
{
  va_list ap;
  
# 114 "./abi/pr77728-2.C" 3 4
 __builtin_va_start(
# 114 "./abi/pr77728-2.C"
 ap
# 114 "./abi/pr77728-2.C" 3 4
 ,
# 114 "./abi/pr77728-2.C"
 n
# 114 "./abi/pr77728-2.C" 3 4
 )
# 114 "./abi/pr77728-2.C"
                 ;
  int x = 
# 115 "./abi/pr77728-2.C" 3 4
         __builtin_va_arg(
# 115 "./abi/pr77728-2.C"
         ap
# 115 "./abi/pr77728-2.C" 3 4
         ,
# 115 "./abi/pr77728-2.C"
         int
# 115 "./abi/pr77728-2.C" 3 4
         )
# 115 "./abi/pr77728-2.C"
                         ;
  
# 116 "./abi/pr77728-2.C" 3 4
 __builtin_va_end(
# 116 "./abi/pr77728-2.C"
 ap
# 116 "./abi/pr77728-2.C" 3 4
 )
# 116 "./abi/pr77728-2.C"
            ;
  return x;
}

int
fn11 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, K<0> n, ...)
{
  va_list ap;
  
# 124 "./abi/pr77728-2.C" 3 4
 __builtin_va_start(
# 124 "./abi/pr77728-2.C"
 ap
# 124 "./abi/pr77728-2.C" 3 4
 ,
# 124 "./abi/pr77728-2.C"
 n
# 124 "./abi/pr77728-2.C" 3 4
 )
# 124 "./abi/pr77728-2.C"
                 ;
  int x = 
# 125 "./abi/pr77728-2.C" 3 4
         __builtin_va_arg(
# 125 "./abi/pr77728-2.C"
         ap
# 125 "./abi/pr77728-2.C" 3 4
         ,
# 125 "./abi/pr77728-2.C"
         int
# 125 "./abi/pr77728-2.C" 3 4
         )
# 125 "./abi/pr77728-2.C"
                         ;
  
# 126 "./abi/pr77728-2.C" 3 4
 __builtin_va_end(
# 126 "./abi/pr77728-2.C"
 ap
# 126 "./abi/pr77728-2.C" 3 4
 )
# 126 "./abi/pr77728-2.C"
            ;
  return x;
}

int
fn12 (int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, K<2> n, ...)
{
  va_list ap;
  
# 134 "./abi/pr77728-2.C" 3 4
 __builtin_va_start(
# 134 "./abi/pr77728-2.C"
 ap
# 134 "./abi/pr77728-2.C" 3 4
 ,
# 134 "./abi/pr77728-2.C"
 n
# 134 "./abi/pr77728-2.C" 3 4
 )
# 134 "./abi/pr77728-2.C"
                 ;
  int x = 
# 135 "./abi/pr77728-2.C" 3 4
         __builtin_va_arg(
# 135 "./abi/pr77728-2.C"
         ap
# 135 "./abi/pr77728-2.C" 3 4
         ,
# 135 "./abi/pr77728-2.C"
         int
# 135 "./abi/pr77728-2.C" 3 4
         )
# 135 "./abi/pr77728-2.C"
                         ;
  
# 136 "./abi/pr77728-2.C" 3 4
 __builtin_va_end(
# 136 "./abi/pr77728-2.C"
 ap
# 136 "./abi/pr77728-2.C" 3 4
 )
# 136 "./abi/pr77728-2.C"
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
