//type:fp
//options_all:--microsoft
//remark:[6.7] Microsoft compatibility: dllexport and function instantiation
// 6/19/24  [EDGcpfe/23516,EDGcpfe/27371]
//
// Microsoft compatibility: dllexport and function instantiation
//
// The front end previously instantiated member functions of template classes
// that are marked dllexport (in Microsoft C++ mode).  It no longer does that.
//
// This example previously elicited an error because S<int>::f() was instantiated
// but that instantiation refers to an undeclared identifier.  Now the example
// is accepted (S<int>::f() is not instantiated).
template<typename> struct __declspec(dllexport) S {
  void f() { undefined_id; }
};
S<int> si;  // Previously triggered an error.  Now okay.
