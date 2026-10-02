//type: fn
//options: 
# 1 "SemaCXX/null_in_arithmetic_ops.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 482 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/null_in_arithmetic_ops.cpp" 2

# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 1
# 84 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_header_macro.h" 1
# 85 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2



# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_ptrdiff_t.h" 1
# 18 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_ptrdiff_t.h"
typedef long int ptrdiff_t;
# 89 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_size_t.h" 1
# 18 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_size_t.h"
typedef long unsigned int size_t;
# 94 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2
# 103 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_wchar_t.h" 1
# 104 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_null.h" 1
# 109 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_nullptr_t.h" 1
# 114 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2
# 123 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_max_align_t.h" 1
# 19 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_max_align_t.h"
typedef struct {
  long long __clang_max_align_nonce1
      __attribute__((__aligned__(__alignof__(long long))));
  long double __clang_max_align_nonce2
      __attribute__((__aligned__(__alignof__(long double))));
} max_align_t;
# 124 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_offsetof.h" 1
# 129 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2
# 3 "SemaCXX/null_in_arithmetic_ops.cpp" 2

void f() {
  int a;
  bool b;
  void (^c)();
  class X;
  void (X::*d) ();
  extern void e();
  int f[2];
  const void *v;

  a = 0 ? __null + a : a + __null;
  a = 0 ? __null - a : a - __null;
  a = 0 ? __null / a : a / __null;

  a = 0 ? __null * a : a * __null;
  a = 0 ? __null >> a : a >> __null;
  a = 0 ? __null << a : a << __null;
  a = 0 ? __null % a : a % __null;

  a = 0 ? __null & a : a & __null;
  a = 0 ? __null | a : a | __null;
  a = 0 ? __null ^ a : a ^ __null;



  v = 0 ? __null + &a : &a + __null;
  v = 0 ? __null + c : c + __null;


  v = 0 ? __null + d : d + __null;


  v = 0 ? __null + e : e + __null;
  v = 0 ? __null + f : f + __null;
  v = 0 ? __null + "f" : "f" + __null;


  a = __null + __null;
  a = __null - __null;
  a = __null / __null;

  a = __null * __null;
  a = __null >> __null;
  a = __null << __null;
  a = __null % __null;

  a = __null & __null;
  a = __null | __null;
  a = __null ^ __null;

  a += __null;
  a -= __null;
  a /= __null;

  a *= __null;
  a >>= __null;
  a <<= __null;
  a %= __null;

  a &= __null;
  a |= __null;
  a ^= __null;

  b = a < __null || a > __null;
  b = __null < a || __null > a;
  b = a <= __null || a >= __null;
  b = __null <= a || __null >= a;
  b = a == __null || a != __null;
  b = __null == a || __null != a;

  b = &a < __null || __null < &a || &a > __null || __null > &a;
  b = &a <= __null || __null <= &a || &a >= __null || __null >= &a;
  b = &a == __null || __null == &a || &a != __null || __null != &a;

  b = 0 == a;
  b = 0 == &a;

  b = __null < __null || __null > __null;
  b = __null <= __null || __null >= __null;
  b = __null == __null || __null != __null;

  b = ((__null)) != a;


  b = c == __null || __null == c || c != __null || __null != c;
  b = d == __null || __null == d || d != __null || __null != d;
  b = e == __null || __null == e || e != __null || __null != e;
  b = f == __null || __null == f || f != __null || __null != f;
  b = "f" == __null || __null == "f" || "f" != __null || __null != "f";

  return __null;
}
