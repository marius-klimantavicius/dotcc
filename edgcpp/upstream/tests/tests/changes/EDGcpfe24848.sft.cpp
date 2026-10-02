//type:fp
//options_all:--c++14 --il --no_il_lowering
//remark:[6.3] Incorrect end position for integral constant in-class field initializers
// 11/26/21 [EDGcpfe/24848]
//
// Incorrect end position for integral constant in-class field initializers
//
// The front end would provide incorrect end-position information for in-class
// field initializers when that initializer is an integral constant.
//
// This is now fixed.
int foo();
class C
{
  int l = 10;    // Previously incorrect end position
  int m = foo(); // Already correct end position
};
