//type: fp
//options: 
# 0 "./warn/Wunused-parm-3.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Wunused-parm-3.C"





# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 7 "./warn/Wunused-parm-3.C" 2


# 8 "./warn/Wunused-parm-3.C"
struct A
{
  long a;
  A () : a (0) { }
  A (long x) : a (x) { }
  operator long () const { return a; }
  long operator- (const A& x) const { return a - x.a; }
};

long
fn1 (A a)
{
  return a - A (0);
}

struct B
{
  bool operator() (const int x, const int y) const throw() { return x < y; }
};

template <typename T>
bool
fn2 (int x, int y, T z)
{
  return z (x, y);
}

bool
fn3 (void)
{
  return fn2 (1, 2, B ());
}

int
fn4 (va_list ap)
{
  return 
# 44 "./warn/Wunused-parm-3.C" 3 4
        __builtin_va_arg(
# 44 "./warn/Wunused-parm-3.C"
        ap
# 44 "./warn/Wunused-parm-3.C" 3 4
        ,
# 44 "./warn/Wunused-parm-3.C"
        int
# 44 "./warn/Wunused-parm-3.C" 3 4
        )
# 44 "./warn/Wunused-parm-3.C"
                        ;
}

template <typename T>
T
fn5 (va_list ap)
{
  return 
# 51 "./warn/Wunused-parm-3.C" 3 4
        __builtin_va_arg(
# 51 "./warn/Wunused-parm-3.C"
        ap
# 51 "./warn/Wunused-parm-3.C" 3 4
        ,
# 51 "./warn/Wunused-parm-3.C"
        T
# 51 "./warn/Wunused-parm-3.C" 3 4
        )
# 51 "./warn/Wunused-parm-3.C"
                      ;
}

int
fn6 (va_list ap)
{
  return fn5 <int> (ap);
}

template <typename T>
int
fn7 (T ap)
{
  return 
# 64 "./warn/Wunused-parm-3.C" 3 4
        __builtin_va_arg(
# 64 "./warn/Wunused-parm-3.C"
        ap
# 64 "./warn/Wunused-parm-3.C" 3 4
        ,
# 64 "./warn/Wunused-parm-3.C"
        int
# 64 "./warn/Wunused-parm-3.C" 3 4
        )
# 64 "./warn/Wunused-parm-3.C"
                        ;
}

int
fn8 (va_list ap)
{
  return fn7 (ap);
}
