//type: fp
//options:  --c++20 --modules
# 0 "./modules/lto-1_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/lto-1_b.C"




# 1 "./modules/lto-1.h" 1
template <typename> struct S {
  S() {}
};
template <typename> inline int x = 0;

extern template struct S<char>;
extern template int x<char>;

template <typename> int* foo() {
  static int x;
  return &x;
};
extern template int* foo<char>();
# 6 "./modules/lto-1_b.C" 2

S<char> s;
int y = x<char>;
int* p = foo<char>();
