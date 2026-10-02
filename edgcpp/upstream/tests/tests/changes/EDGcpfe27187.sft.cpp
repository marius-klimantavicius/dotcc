//type:fp
//options_all:--c++20
//remark:[6.7] Assertion failure on friend function redeclaration in class template
// 4/22/24  [EDGcpfe/27187]
//
// Assertion failure on friend function redeclaration in class template
//
// In some cases where a friend function is defined in one class template and also
// declared in another class template, instantiating that friend function could
// result in a failed assertion in scan_function_body.
template<int I> struct C2;
template<int I>
struct C1 {
  friend int f(C1, C2<I>) { return 1; }
};
template<int I>
struct C2 {
  friend int f(C1<I>, C2);
};
int i = f(C1<0>(), C2<0>());  // Previously triggered an assertion failure.
                              // Now okay.
