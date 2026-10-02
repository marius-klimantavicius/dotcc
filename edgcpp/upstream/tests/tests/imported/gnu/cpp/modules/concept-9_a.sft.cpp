//type: fp
//options:  --c++20 --modules --c++20
# 0 "./modules/concept-9_a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/concept-9_a.C"





module;

# 1 "./modules/concept-9.h" 1


template <typename>
concept foo = false;

template <typename>
concept bar = true;

template <typename T>
struct corge {};

template <foo F>
struct corge<F> {};

template <bar B>
struct corge<B> {
  using alias = int;
};
# 9 "./modules/concept-9_a.C" 2

export module M;

export template<class T>
using corge_alias = corge<T>::alias;
