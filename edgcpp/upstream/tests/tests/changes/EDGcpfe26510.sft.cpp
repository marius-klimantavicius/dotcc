//type:fp
//options_all:--ms_c++20
//remark:[6.8] MSVC and Clang compatibility: substitution of conditional explicit specifiers
// 5/26/25  [EDGcpfe/26510,EDGcpfe/26763,EDGcpfe/28158]
//
// MSVC and Clang compatibility: substitution of conditional explicit specifiers
//
// MSVC and Clang do not yet implement the resolution of Core issue 2369, which
// mandates constraint checking before substitution.  However, MSVC and Clang 18+
// substitute into the conditional explicit specifier of a constructor only after
// checking the associated constraints.
template<bool B>
struct A {
  static_assert(B);  // Previously a spurious error.  Now okay.
};
struct C {
  template<typename T> requires (sizeof(T) != sizeof(int))
  explicit(A<sizeof(T) != sizeof(int)>::v) C(T);
  C(long);
};
C c = 1;
