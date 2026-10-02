//type:fp
//options_all:--c++11
//remark:[4.4] Parameters following an empty function parameter pack expansion
// 5/14/11  [EDGcpfe/11560]
//
// Parameters following an empty function parameter pack expansion
//
// The front end previously issued a spurious error when an empty function
// parameter pack expansion was followed by another parameter declaration.
//
// This is now fixed.
//
// 5/14/11  [EDGcpfe/11560]
//
// Default arguments on members of variadic class templates
//
// In modes that accept variadic templates (like C++0x mode), the front end now
// accepts a default argument on a parameter of a member function of a class
// template that is a pack expansion.
//
// Note that default arguments are not valid on function parameter packs for
// function and member function templates.  E.g.:
template<typename ... T> struct S {
  void f(T ... p, int x);
};
S<> s;  // Empty pack expansion previously triggered an error on the
        // declaration of parameter x.  Now okay.
