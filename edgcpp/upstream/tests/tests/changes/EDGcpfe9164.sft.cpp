//type:fp
//options_all:--c++11
//remark:[4.8] C++11: Inheriting constructors
// 8/12/13  [EDGcpfe/9164]
//
// C++11: Inheriting constructors
//
// In C++11 mode, the front end now accepts using declarations in derived
// classes that denote the implicit generation of "inheriting constructors", as
// described in the C++ standards committee's paper N2540.
//
// The acceptance of this feature is controlled by the global variable
// inheriting_constructors_enabled.
struct B {
  B(int);
  B(double);
};
struct D: B {
  using B::B;  // Generates D::D(double), but not D::D(int).
  D(int);
};
