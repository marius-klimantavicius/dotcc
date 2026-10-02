//type:fp
//options_all:--c++11
//remark:[4.10] Spurious error on default argument following a function parameter pack
// 12/2/14  [EDGcpfe/14566]
//
// Spurious error on default argument following a function parameter pack
//
// A spurious error had been given on the use of a default argument following
// a function parameter pack.  Now fixed.
struct A {
  template <class ...T> A(T..., int=0);
};
