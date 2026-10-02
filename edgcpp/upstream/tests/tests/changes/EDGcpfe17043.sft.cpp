//type:fp
//options_all:--c++11
//remark:[4.12] Spurious error on call to template member that is later overloaded
// 7/13/16  [EDGcpfe/17043]
//
// Spurious error on call to template member that is later overloaded
//
// The front end sometimes failed to match an out-of-class definition to its
// in-class declaration if the two declarations involve a call to an overloaded
// set of which only one member is declared at the point of the in-class
// declaration.
//
// This is now fixed.
template<typename T> struct X {
  auto f() -> void;
  auto g() -> decltype(this->f());
  template<typename> auto f() const -> void;
};
template<typename T>
auto X<T>::g() -> decltype(this->f()) {}
  // Previously triggered an incompatibility error.  Now okay.
