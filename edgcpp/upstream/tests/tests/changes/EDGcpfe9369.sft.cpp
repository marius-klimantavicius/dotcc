//type:fp
//options_all:--microsoft
//remark:[4.0] Microsoft C++ compatibility: dllimport/dllexport and explicit instantiations
// 11/24/08 [EDGcpfe/9369]
//
// Microsoft C++ compatibility: dllimport/dllexport and explicit instantiations
//
// In Microsoft C++ mode, explicitly instantiating an ordinary (i.e., nontemplate)
// member function of a class template applies the dllimport/dllexport specifiers
// from the template to the instantiation directive if no such specifiers appeared
// on the directive itself.  This includes the "do not instantiate" meaning of the
// dllimport specifier.
struct I;
template<typename T> struct __declspec(dllimport) X {
  void f(T *p) { p->i(); }
};
template void X<I>::f(I*);  // Treated as if __declspec(dllimport) appeared
                            // on this directive (and therefore no error is
                            // issued even though struct I is incomplete).
