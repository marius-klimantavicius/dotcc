//type:fp
//options_all:--c++14 --gnu_version 40900
//remark:[6.2] Overloading static and nonstatic member function templates
// 9/4/20   [EDGcpfe/19987,EDGcpfe/23358]
//
// Overloading static and nonstatic member function templates
//
// The standard does not permit overloading static and nonstatic member function
// templates with identical parameter types, trailing requires-clauses, and
// template parameterization.  However, Clang and GCC appear to enforce that rule
// only if the return types are also identical.  The front end now emulates that
// behavior in GNU and Clang modes.
struct S {
  template<typename T> void f(int);
  template<typename T> static int f(int);
    // Previously always an error.  Now accepted in GNU and Clang modes.
};
