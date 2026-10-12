template<class T, int N = 3> struct ParameterBase { T value; };
template<class T> struct Selected { static constexpr int tag = 0; };
template<class T> struct Selected<T *> { static constexpr int tag = 1; };
template<> struct Selected<int> { static constexpr int tag = 2; };
template<template<class, int> class C, class T> struct Rebound { C<T, 4> inner; };
template<class... T> struct TypePack {};
template<int... N> constexpr int folded_sum = (N + ... + 0);
template<class... T> auto captured_pack(T... values) {
  return [...captured = values]() { return (captured + ... + 0); };
}
template<class T> concept Sized = requires { sizeof(T); };
template<class T> concept Addable = requires(T left, T right) {
  { left + right } noexcept -> Sized;
  requires sizeof(T) > 0;
};
template<class T> constexpr bool dependent_concept = Addable<T>;
constexpr bool concrete_concept = Addable<int>;
template<Addable T> T constrained_add(T left, T right) { return left + right; }
template<class... T> constexpr decltype(sizeof(0)) pack_size() { return sizeof...(T); }
template<class T> constexpr int dependent_member() { return Selected<T *>::tag; }
template<class T> T local_copy(T argument) { T local = argument; return local; }

Selected<int *> selected_pointer;
Selected<int> selected_exact;
Rebound<ParameterBase, int> rebound;
TypePack<int, long, char> populated_pack;
TypePack<> empty_pack;
int template_edges() {
  auto closure = captured_pack(1, 2, 3);
  return selected_pointer.tag + selected_exact.tag + folded_sum<1, 2, 3>
    + constrained_add(4, 5) + closure() + pack_size<int, char>()
    + dependent_member<int>() + rebound.inner.value;
}

#if defined(__cpp_pack_indexing)
template<decltype(sizeof(0)) I, class... T> using PackElement = T...[I];
PackElement<1, char, long> indexed_type;
#endif
