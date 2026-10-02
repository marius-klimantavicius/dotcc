//type:fn
//remark:[4.5] Abort on invalid member using-declaration in class template
// 8/28/12  [EDGcpfe/13155]
//
// Abort on invalid member using-declaration in class template
//
// Certain invalid member using-declarations in class templates aborted with an
// internal error in member_using_or_alias_declaration (class_decl.c).
//
// This is now fixed.  (Note that while such cases are normally invalid they are
// usually accepted in GNU and Microsoft modes.)
template<class T> struct S {
  typedef int I;
  struct N {
    using typename S<T>::I;  // Error.  Previously triggered an
  };                         // internal error.
};
