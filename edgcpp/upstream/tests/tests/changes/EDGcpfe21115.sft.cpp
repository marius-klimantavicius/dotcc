//type:fp
//options_all:--c++17
//remark:[5.1] Defaulted parameters having the parent class type in copy constructors
// 5/27/19  [EDGcpfe/21115]
//
// Defaulted parameters having the parent class type in copy constructors
//
// In C++14 and earlier, a copy constructor that has a defaulted parameter of the
// same type as the class being constructed would use the copy constructor to copy
// the default argument value to the parameter variable, resulting in unbounded
// recursion, and was therefore not permitted.  In C++17, however, guaranteed copy
// elision means that that copy may be elided, making such parameters potentially
// valid.  The front end now accepts copy constructors with defaulted arguments
// that have the parent class type in these modes.
//
// This is now fixed.
struct B {
  B();
  B(const B &, B = B()); // Rejected in C++14 mode
                         // (Now) accepted in C++17 mode
};
void f() {
  B b;
  B b2(b);
}
