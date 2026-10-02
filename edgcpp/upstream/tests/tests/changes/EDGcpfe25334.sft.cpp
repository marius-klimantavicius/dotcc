//type:fp
//options_all:--c++20
//remark:[6.4] Mangling of requires expressions
// 5/31/22  [EDGcpfe/25334]
//
// Mangling of requires expressions
//
// When generating mangled names for code that uses concepts, neither of the
// ABIs require mangling of a requires expression, but in some non-C-generating
// configurations where MANGLE_ALL_NAMES is TRUE, an assertion failure had
// occurred.  The assertion failure has been replaced by a discretionary error
// (which is emitted only once per compilation).
template<class T, T t> struct A {
  static constexpr T value = t;
};
template<class T, T t> constexpr T A<T, t>::value;
template<bool t> using B = A<bool, t>;
template<class T> struct S { };
template<class T>
  requires __is_enum(T)
  struct S<T>
  : B<!requires(T t, void(*f)(int)) { f(t); }>
  { };
