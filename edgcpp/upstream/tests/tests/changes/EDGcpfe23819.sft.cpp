//type:fp
//options_all:--c++20
//remark:[6.2] Class with a constexpr destructor and anonymous union not treated as literal
// 1/27/21  [EDGcpfe/23819]
//
// Class with a constexpr destructor and anonymous union not treated as literal
//
// The front end previously always treated a class with a constexpr destructor and
// an anonymous union as a non-literal class type.  That in turn could result in
// spurious errors.
//
// That is now fixed.
template<typename T> struct S {
  union { T object; };
  constexpr ~S() {}
};
struct N {
  constexpr ~N() {}
};
constexpr void f() {
  S<N> s;  // Previously an error because S<N> considered non-literal.
}          // Now okay.
