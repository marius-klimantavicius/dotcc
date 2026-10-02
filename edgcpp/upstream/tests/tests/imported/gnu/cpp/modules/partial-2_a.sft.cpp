//type: fp
//options:  --c++20 --modules
# 0 "./modules/partial-2_a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/partial-2_a.C"



export module pr106826;

# 1 "./modules/partial-2.h" 1
template<class T> constexpr bool is_reference_v = false;
template<class T> constexpr bool is_reference_v<T&> = true;
template<class T> constexpr bool is_reference_v<T&&> = true;

struct A {
  template<class T> static constexpr bool is_reference_v = false;
};

template<class T> constexpr bool A::is_reference_v<T&> = true;
template<class T> constexpr bool A::is_reference_v<T&&> = true;
# 7 "./modules/partial-2_a.C" 2
