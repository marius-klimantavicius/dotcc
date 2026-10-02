//type:fp
//options_all:--c++14
//remark:[6.4] Assertion failure in mangled_simple_id
// 8/12/22  [EDGcpfe/25485]
//
// Assertion failure in mangled_simple_id
//
// The mangling of a captured "this" had caused an assertion failure in
// mangled_simple_id.  That is now fixed.
template <class> bool v;
template <class T> struct A {
  typename T::X mf() {
    [&]() { v<decltype(mf())>; };
  }
};
