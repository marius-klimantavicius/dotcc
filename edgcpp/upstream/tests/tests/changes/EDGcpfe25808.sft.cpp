//type:fp
//options_all:--gnu=110000
//remark:[6.5] GNU/Clang compatibility: abi_tag attribute on friend definition
// 11/30/22 [EDGcpfe/25808]
//
// GNU/Clang compatibility: abi_tag attribute on friend definition
//
// A spurious error had been given on a friend declaration that is a definition
// with an abi_tag attribute.  Now fixed.
template <class T> struct A {
  __attribute__((__abi_tag__("v15000"))) friend A f() {}
};
