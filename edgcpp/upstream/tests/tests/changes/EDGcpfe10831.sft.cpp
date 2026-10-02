//type:fp
//options_all:--microsoft
//remark:[4.2] Microsoft compatibility: "static" specifier on out-of-class member declaration
// 7/14/10  [EDGcpfe/10831]
//
// Microsoft compatibility: "static" specifier on out-of-class member declaration
//
// Ordinarily, the storage specifier "static" cannot be specified on an out-of-
// class member declaration (not even if it is a static member).  Now, however,
// if the declaration is a template declaration in Microsoft bugs mode, the
// "static" keyword is ignored with a warning.
template<class T> struct S { void f(); };
template<class T> static void S<T>::f() {}
  // Previously always an error.  Now only elicits a warning in
  // Microsoft bugs mode.
