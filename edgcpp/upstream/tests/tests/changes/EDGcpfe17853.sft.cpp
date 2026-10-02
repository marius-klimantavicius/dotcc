//type:fp
//options_all:--c++11
//remark:[4.13] Dependent exception specifications and redeclarations
// 12/19/16 [EDGcpfe/17853]
//
// Dependent exception specifications and redeclarations
//
// The front end previously issued a spurious compatibility error on some
// redeclarations of a function template with a dependent exception specification
// containing an unqualified call to an unknown function if a candidate for that
// function was introduced in an unnamed namespace following the initial
// declaration.
//
// This is now fixed.
namespace N {
  template<typename T> void f(T p) noexcept(noexcept(f(p)));
  namespace { void f(); }
  template<typename T> void f(T p) noexcept(noexcept(f(p)));
    // Previously triggered a spurious redeclaration error.  Now okay.
}
