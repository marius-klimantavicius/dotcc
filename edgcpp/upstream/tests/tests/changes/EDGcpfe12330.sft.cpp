//type:fp
//remark:[4.4] Spurious error on a dependent friend constructor declaration
// 10/28/11 [EDGcpfe/12330]
//
// Spurious error on a dependent friend constructor declaration
//
// Previously, attempting to name a constructor of a class template in a friend
// template declaration for another class template triggered a spurious error
// (the specific error depended on the mode).
//
// This is now fixed.
template <typename T> struct A { A() {} };
template <typename S> struct B {
  template<typename T> friend A<T>::A();  // Previously a spurious error.
};
