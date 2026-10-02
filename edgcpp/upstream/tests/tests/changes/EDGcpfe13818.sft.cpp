//type:fp
//options_all:--c++11
//remark:[4.6] Enum types defined in templates
// 3/27/13  [EDGcpfe/13818]
//
// Enum types defined in templates
//
// Enumeration types defined in templates were previously never considered
// template-dependent.  However, when such a type was created during the
// prototype instantiation of a template in configurations with
// PROTOTYPE_INSTANTIATIONS_IN_IL set to FALSE, it could not be given a valid
// parent scope (since the parent scope is not part of the IL).  This in turn
// could lead to aborts (in particular in lowering and name mangling).
//
// This is now fixed: Enumeration types defined in prototype instantiation are
// now treated as dependent types.
template <class T> void f(T *) {}
template<class T> struct S {
  void g() {
    enum { x = sizeof(T) } e;  // Previously a non-dependent type.
    f(&e);                     // Previously triggered an abort in
  }                            // IL lowering (in some configurations).
};
