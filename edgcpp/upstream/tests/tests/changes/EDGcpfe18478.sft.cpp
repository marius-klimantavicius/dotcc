//type:fp
//options_all:--ms_c++17
//remark:[6.7] Declaration matching for enums in Microsoft nonreal base classes
// 9/17/24  [EDGcpfe/18478,EDGcpfe/22305,EDGcpfe/27354]
//
// Declaration matching for enums in Microsoft nonreal base classes
//
// In Microsoft permissive mode, an out-of-class function definition with a
// parameter of enum type where the enumeration is a member of a dependent base
// class could result in a spurious incompatible declaration error.
// with --ms_c++17:
template<typename T> struct B {
  enum E { };
};
template<typename T> struct C : public B<T> {
  void f(enum B<T>::E);
};
template<typename T>
void C<T>::f(enum B<T>::E) { }  // Previously a spurious error in Microsoft
                                // permissive mode.  Now okay.
