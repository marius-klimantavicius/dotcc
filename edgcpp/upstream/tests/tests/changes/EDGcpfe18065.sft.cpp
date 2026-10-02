//type:fp
//options_all:--gnu_version 50200 --c++14
//remark:[4.14] GNU C++14-mode abort on member variable template
// 3/8/17   [EDGcpfe/18065]
//
// GNU C++14-mode abort on member variable template
//
// The changes for EDGcpfe/18004 introduced a bug causing the front end to abort
// (in ensure_inclass_static_member_constant_initializer_is_scanned, class_decl.c)
// with an internal error in GNU C++14 mode when an expression refers to a member
// variable template.
//
// This regression introduced in version 4.13 is now fixed.
struct S {
  template<typename T> static typename T::type vt;
};
struct X {
  using type = int;
};
int *p = &S::vt<X>;  // Previously triggered an internal error in
                     // GNU C++14 mode.
