//type:fp
//options_all:--c++11
//remark:[5.0] Spurious error on attribute applied to friend definition in class template
// 4/20/18  [EDGcpfe/19388]
//
// Spurious error on attribute applied to friend definition in class template
//
// A spurious error had been emitted when an attribute appertained to a
// friend definition in a class template.
template <class T> struct S {
  [[noreturn]] friend int f() { while(1); }
};
