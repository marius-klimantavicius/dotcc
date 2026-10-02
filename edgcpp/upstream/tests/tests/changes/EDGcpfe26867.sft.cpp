//type:fp
//options_all:--c++20
//remark:[6.7] Template parameter pack in requires clause of friend template declaration
// 12/19/23 [EDGcpfe/26867]
//
// Template parameter pack in requires clause of friend template declaration
//
// Previously, the front end considered a requires clause for a friend template
// declaration containing a parameter pack in some contexts to be incompatible
// with the corresponding out-of-class declaration.
template<typename ...> concept C = true;
struct A {
  template<typename ... T>
  requires (C<T> && ...)
  friend class B;
};
template<typename ... T>
requires (C<T> && ...)  // Previously a spurious error.  Now okay.
class B;
