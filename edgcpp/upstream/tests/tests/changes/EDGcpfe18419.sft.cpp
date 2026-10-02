//type:fp
//options_all:--clang
//remark:[4.14] Spurious error on __builtin_addressof in template
// 5/22/17  [EDGcpfe/18419]
//
// Spurious error on __builtin_addressof in template
//
// During prototype instantiations, an invocation of __builtin_addressof (see
// the entry for EDGcpfe/16955) could produce a spurious error about its operand
// not being an lvalue.
//
// This is now fixed.
template<typename T> struct S { int i; };
template<typename T> void f(S<T> *p) {
  __builtin_addressof(p->i);  // Previously elicited a spurious error.
}                             // Now okay.
template void f(S<int>*);
