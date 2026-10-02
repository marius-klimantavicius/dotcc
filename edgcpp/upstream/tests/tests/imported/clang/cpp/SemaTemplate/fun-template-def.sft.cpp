//type: fn
//options:  --c++03: --c++11: --c++17: --c++20
# 1 "SemaTemplate/fun-template-def.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 431 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaTemplate/fun-template-def.cpp" 2
# 10 "SemaTemplate/fun-template-def.cpp"
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
# 128 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_offsetof.h" 1
# 129 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2
# 11 "SemaTemplate/fun-template-def.cpp" 2


namespace std { class type_info {}; }

struct dummy {};




template<typename T>
int f0(T x) {
  return (sizeof(x) == sizeof(int))? 0 : (sizeof(x) == sizeof(double))? 1 : 2;
}

template <typename T, typename U>
T f1(T t1, U u1, int i1, T** tpp)
{
  T t2 = i1;
  t2 = i1 + u1;
  ++u1;
  u1++;
  int i2 = u1;

  i1 = t1[u1];
  i1 *= t1;

  i1(u1, t1);
  u1(i1, t1);

  U u2 = (T)i1;
  static_cast<void>(static_cast<U>(reinterpret_cast<T>(
    dynamic_cast<U>(const_cast<T>(i1)))));

  new U(i1, t1);
  new int(t1, u1);
  new (t1, u1) int;
  delete t1;

  dummy d1 = sizeof(t1);
  dummy d2 = __builtin_offsetof(T, foo);
  dummy d3 = __alignof(u1);
  i1 = typeid(t1);
  i1 = tpp[0].size();

  return u1;
}

template<typename T>
void f2(__restrict T x) {}

void f3() {
  f2<int*>(0);
  f2<int>(0);
}
