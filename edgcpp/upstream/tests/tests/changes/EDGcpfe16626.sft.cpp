//type:fp
//options_all:--c++11
//remark:[4.11] Spurious error on template-dependent functional-notation cast with braces
// 11/9/15  [EDGcpfe/16626]
//
// Spurious error on template-dependent functional-notation cast with braces
//
// When a C++11-style functional-notation cast with braces appears in a context
// requiring a constant result (such as a template argument), the front end
// sometimes issued a spurious error.
//
// This is now fixed.
template<bool> struct M { typedef int type; };
template <typename> struct B {
  constexpr operator bool() const noexcept { return true; }
};
template <typename T,
          typename = typename M<B<T>{}>::type> struct S;
  // Parsing of the default argument previously produced a spurious error
  // implying that the cast isn't constant.  Now okay.
