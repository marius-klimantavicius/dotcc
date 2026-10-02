//type:fp
//remark:[4.10] Spurious constant expression errors in non-type template arguments
// 6/10/14  [EDGcpfe/15084,EDGcpfe/15145,EDGcpfe/15220]
//
// Spurious constant expression errors in non-type template arguments
//
// The front end previously incorrectly rejected some expressions used as
// non-type template arguments, either because they use the unary *
// indirection operator or because a dependent expression was claimed to be
// non-constant.  This has now been fixed.  In particular, the unary *
// operator is now accepted in C++11 and Microsoft modes when applied to an
// address constant designating a variable that can be used in a constant
// expression, and dependent expressions in a template definition are treated
// as constant.
template <int I> class A { };
template <typename T> void f() {
  const int c = T::x;
  const int d = T::y - c;
  A<d> a;   // Previously an error
}

template <const int *P> struct B : A<*P> {};   // Previously an error
extern const int N = 0;
B<&N> foo;
