//type:fp
//options_all:--c++11 --gnu=50000
//remark:[4.11] GNU compatibility: Spurious error on abi_tag attribute applied to lambda
// 1/25/16  [EDGcpfe/16798]
//
// GNU compatibility: Spurious error on abi_tag attribute applied to lambda
//
// A spurious error had been given when an abi_tag attribute appertained to
// a lambda.  Now fixed.
void f() {
  []() __attribute__((abi_tag("test"))) {}();
}
