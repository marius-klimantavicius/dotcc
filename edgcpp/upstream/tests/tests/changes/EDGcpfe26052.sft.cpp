//type:fp
//options_all:--c++14
//remark:[6.5] Abort on out-of-class partial specialization of static data member template
// 4/18/23  [EDGcpfe/26052]
//
// Abort on out-of-class partial specialization of static data member template
//
// Previously, the front end aborted with a failed assertion in
// enclosing_class_type for an out-of-class partial specialization declaration of
// a static data member template.
template<typename T>
struct C {
  template<typename U>
  static T s;
};
template<typename T> template<typename U>
T C<T>::s<U *> = 1;  // Previously aborted during instantiation.  Now okay.
int i = C<int>::s<int *>;
