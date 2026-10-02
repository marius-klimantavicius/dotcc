//type:fp
//options_all:--c++23
//remark:C++23: CTAD for inheriting constructors
// 5/11/26  [EDGcpfe/25516]
//
// C++23: CTAD for inheriting constructors
//
// The front end now supports class template argument deduction (CTAD) for
// inheriting constructors, as specified in WG21 paper P2582R1.
// --c++23:
//
// The IL routine entry for a deduction guide generated from an inheriting
// constructors has its is_deduction_guide_from_inheriting_ctor flag set to TRUE.
template<typename T>
struct B {
  B(T);
};
template<typename T>
struct D : B<T> {
  using B<T>::B;
};
D d(1);  // Now deduced as D<int>
