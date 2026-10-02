//type:fp
//options_all:--microsoft
//remark:[4.1] Microsoft C++ compatibility: dllimport/dllexport and explicit instantiations
// 12/11/08 [EDGcpfe/9394]
//
// Microsoft C++ compatibility: dllimport/dllexport and explicit instantiations
//
// Version 4.0 modified the effect of dllimport/dllexport on member functions of
// class templates when explicitly instantiating those member functions (see the
// Changes entry for EDGcpfe/9369 on 11/24/08).  Specifically, dllexport/dllimport
// applied to the class template is carried over to the explicit instantiation
// if the instantiation does not itself specify dllimport or dllexport.  However,
// this behavior was unintentionally disabled if the instantiated class member was
// overloaded with other members.
//
// This is now fixed.
struct I;
template<typename T> struct __declspec(dllimport) X {
  void f(T *p) { p->i(); }
  void f();                 // f is now overloaded.
};
template void X<I>::f(I*);  // Previously, this ignored dllimport because
                            // X<I>::f is overloaded, which in turn triggered
                            // an error (because I is incomplete).  Now, this
                            // is accepted because dllimport means "do not
                            // instantiate".
