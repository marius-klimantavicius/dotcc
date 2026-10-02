//type:fp
//options_all:--c++11 --microsoft
//remark:[4.6] Abort on initialization of an enum bit field in Microsoft C++11 mode
// 1/7/13   [EDGcpfe/13549]
//
// Abort on initialization of an enum bit field in Microsoft C++11 mode
//
// Initialization a bit field using a braced initializer in Microsoft C++11
// mode could result in an internal error in conv_integer_to_integer
// (folding.c) if the initializer was not a constant value.
//
// This regression was introduced in version 4.5 by the changes for EDGcpfe/9170.
enum E { e };
struct S { E b: 2; };
void g(E x) {
  S s = {x};  // Previously, this could produce an internal error in
}             // Microsoft mode.
