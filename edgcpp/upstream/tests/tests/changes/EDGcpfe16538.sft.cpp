//type:fp
//options_all:--c++11
//remark:[4.11] Spurious errors on nonreal template arguments with constexpr conversions
// 9/25/15  [EDGcpfe/16538]
//
// Spurious errors on nonreal template arguments with constexpr conversions
//
// The front end did not always correctly deal with template arguments of a
// template-dependent class type that, when instantiated, could convert to the
// needed nontype template parameter type through a constexpr conversion.  This
// resulted in spurious errors.
//
// This is now fixed.
template<bool> struct X {};
template<class T> struct B {
  constexpr operator bool() const { return true; }
};
template<typename T> void g(T) {
  X<B<T>()> x;  // Previously triggered spurious errors about B<T>() not
}               // being compatible with bool and not being a constant.
