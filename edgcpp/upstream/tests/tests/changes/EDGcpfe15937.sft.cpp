//type:fp
//remark:[4.12] Spurious error on Microsoft-mode constructor declaration in template
// 5/16/16  [EDGcpfe/15937]
//
// Spurious error on Microsoft-mode constructor declaration in template
//
// Version 4.5 introduced a change that caused certain nonreal classes used as
// base classes to have actual instantiations done on them in Microsoft mode.
// During such instantiations, constructors with explicit template arguments were
// not always handled correctly, causing the front end to emit spurious errors
// claiming the type associated with the constructor name does not match the
// enclosing class type.
//
// This is now fixed.
template<typename T, int N> struct B {
  B<T, N>() {}
};
template<int N> struct D : B<int, N> {};
  // Previously triggered a spurious error on the constructor of B<int, N>.
