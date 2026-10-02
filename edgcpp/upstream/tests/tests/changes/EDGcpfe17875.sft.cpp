//type:fp
//options_all:--microsoft_version 1903
//remark:[4.13] Microsoft-mode abort on inherited nonreal member using-declaration
// 12/21/16 [EDGcpfe/17875]
//
// Microsoft-mode abort on inherited nonreal member using-declaration
//
// In some cases, a member using-declaration involving a nonreal template type
// argument produced an internal error in Microsoft mode (in function
// conflicts_with_previous_function_decl in class_decl.c) when another level of
// derivation adds another using-declaration for the same name.
//
// This is now fixed.
template<typename> struct B {};
template<typename T> struct C: B<T> {
  using B<X>::f;  // X is treated as a nonreal argument in Microsoft mode.
  template<typename> void f() {}
};
template<typename T> struct D: C<T> {
  using C<T>::f;  // Previously triggered an internal error.  Now okay.
};
