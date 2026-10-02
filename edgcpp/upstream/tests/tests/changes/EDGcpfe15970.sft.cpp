//type:fp
//options_all:--g++ --c++11
//remark:[4.10.1] Abort with constexpr base class pointer cast
// 2/16/15  [EDGcpfe/15970]
//
// Abort with constexpr base class pointer cast
//
// The front end previously aborted with a failed assertion (in fold_expr) on
// attempting to cast a derived class object pointer to a pointer to one of
// its bases in a constexpr context.  This is now fixed.
// --g++ --c++11):
class Base { };
struct Derived : public Base {};
struct Nested : public Derived {
  static constexpr Base* f() { return &n; }  // Previously aborted
  static Nested n;
};

void f() {
  Nested::f();
}
