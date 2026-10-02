//type:fp
//options_all:--c++11
//remark:[4.8] Instantiation of class template of non-literal type with a constexpr member
// 7/9/13   [EDGcpfe/14202]
//
// Instantiation of class template of non-literal type with a constexpr member
// function
//
// A constexpr member function is only permitted in a literal class type.  This
// rule is relaxed when the class type is an instantiation of a class template,
// but the front end previously issued an error for such cases.  This error is now
// suppressed and the member function is not considered constexpr.
// (with --c++11):
struct A {
  A() { }
};
template <class T> struct B {
  T m;    // Makes B<A> a non-literal class.
  constexpr int f() { return 0; }
};
B<A> a;
