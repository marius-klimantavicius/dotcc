//type:fp
//options_all:--c++11
//remark:[5.0] IA-64 ABI: Assertion failure in initialize_vptr
// 1/5/18   [EDGcpfe/19015]
//
// IA-64 ABI: Assertion failure in initialize_vptr
//
// A failed assertion (in initialize_vptr) could occur in cases where an aggregate
// constant is being used to initialize an object of class type where the IA-64
// ABI layout rules dictate that the object layout be changed to accommodate
// sharing of a vptr.  Now fixed.
struct B1 {
  int m = 37;
};
struct B2 {
  virtual void f();
};
struct A : B1, B2 {};
struct D : A {} d;
