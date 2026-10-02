//type:fp
//options_all:--c++11
//remark:[4.4] Default arguments on members of variadic class templates
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
  void f(T ... p = 0);  // Previously disallowed in C++0x mode; now okay.
};
S<int, int, int> s;
int main() { s.f(); }
