//type: fn
//options:  --c++11
# 1 "SemaCXX/libcxx_valarray_hack.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/libcxx_valarray_hack.cpp" 2
# 25 "SemaCXX/libcxx_valarray_hack.cpp"
# 1 "SemaCXX/libcxx_valarray_hack.cpp" 1
# 11 "SemaCXX/libcxx_valarray_hack.cpp" 3
namespace std {
  using size_t = long unsigned int;
  template<typename T> struct valarray {
    __attribute__((internal_linkage)) valarray(size_t) {}
    __attribute__((internal_linkage)) ~valarray() {}
  };

  extern template valarray<size_t>::valarray(size_t);
  extern template valarray<size_t>::~valarray();
}
# 26 "SemaCXX/libcxx_valarray_hack.cpp" 2

template<typename T> struct foo {
  __attribute__((internal_linkage)) void x() {};
};
extern template void foo<int>::x();
