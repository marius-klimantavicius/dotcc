//type:fp
//options_all:--c++11
//remark:[6.5] Type equivalence for nested template-ids and error types
// 12/14/22 [EDGcpfe/25093,EDGcpfe/25795]
//
// Type equivalence for nested template-ids and error types
//
// The front end previously considered template-ids with a dependent
// decltype-specifier as a nested name specifier that only differed in the
// decltype-specifier to be equivalent.
//
// A similar problem also existed for error types appearing in alias templates.
// In more complex cases, these problems could result in crashes during template
// substitution.
template<class T> void f(typename decltype(T::A)::template N<int>)
{ }
template<class T> void f(typename decltype(T::B)::template N<int>)
{ }  // Previously a spurious redefinition error.  Now okay.
