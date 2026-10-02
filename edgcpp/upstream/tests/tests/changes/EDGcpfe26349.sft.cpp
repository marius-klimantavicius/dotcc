//type:fp
//options_all:--c++11
//remark:[6.6] Access checking for enumeration-qualified enumerators
// 7/20/23  [EDGcpfe/26349]
//
// Access checking for enumeration-qualified enumerators
//
// As clarified by committee paper P1787R6, access checking should not be
// performed for enumeration-qualified enumerators as they are considered to be
// members of the enumeration.
struct B {
protected:
  enum E { E1 };
};
struct D : B {
  using B::E;
};
auto e = D::E::E1;  // Previously a spurious error. Now okay.
