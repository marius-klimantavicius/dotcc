//type:fp
//options_all:--gn 40600
//remark:[6.2] IA-64 ABI: Assertion failure in add_discriminator
// 10/12/20 [EDGcpfe/23165]
//
// IA-64 ABI: Assertion failure in add_discriminator
//
// In IA-64 ABI configurations that do mangling, an assertion failure (in
// add_discriminator) had occurred when mangling a constant in a local class.
// This regression was introduced by the changes for EDGcpfe/11127,EDGcpfe/21364
// (in version 6.0).
void f() {
  class A {
    const int x = 33;
  };
}
