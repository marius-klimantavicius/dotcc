//type:fp
//options_all:--c++11
//remark:[4.9] C++11: Narrowing conversion and nontype template arguments
// 3/21/14  [EDGcpfe/14936]
//
// C++11: Narrowing conversion and nontype template arguments
//
// In C++11, narrowing conversions are not permitted for nontype template
// arguments of integral or enum type.  However, the front end was previously
// only considering the type of a template argument consisting of the name of
// a constant-valued variable (and not its value) when deciding whether a
// narrowing conversion is involved.  This could trigger spurious errors.
//
// This is now fixed.
template<int T> void g() {}
void f() {
  unsigned const N = 1;
  g<N>();  // A conversion from unsigned to int is not a narrowing
}          // conversion if the unsigned value is a known constant that
           // fits in type int.  Previously the front triggered an error
           // here; now okay.
