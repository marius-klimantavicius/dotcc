//type:fp
//options_all:--c++11
//remark:[4.11] Spurious exception mismatch error on noexcept-specifier in class template
// 12/11/15 [EDGcpfe/16688]
//
// Spurious exception mismatch error on noexcept-specifier in class template
//
// In some cases, a virtual function with a noexcept specifier overriding a
// similar function for a template base class resulted in a spurious error
// reporting an exception specification mismatch.
//
// This is now fixed.
template<typename T> struct B {
  virtual bool f(T) noexcept(false);
};
struct D : B<char> {
  virtual bool f(char) noexcept(false);  // Previously triggered a spurious
};                                       // error.
