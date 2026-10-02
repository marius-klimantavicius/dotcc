//type:fp
//options_all:--c++17
//remark:[5.1] Guaranteed copy elision
// 4/15/19  [EDGcpfe/20538,EDGcpfe/21108]
//
// Guaranteed copy elision
//
// The front end was not always performing copy elision when required by C++17.
//
// This is now fixed.
struct A {
  A();
  A(A &key);
};
auto foo = A{}; // Previously issued a "no suitable copy constructor" error
