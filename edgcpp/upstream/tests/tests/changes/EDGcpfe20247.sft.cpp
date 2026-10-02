//type:fp
//options_all:--c++11
//remark:[5.1] Internal error in i_copy_dynamic_init
// 10/8/18  [EDGcpfe/20247]
//
// Internal error in i_copy_dynamic_init
//
// In some odd cases, as a result of the changes for EDGcpfe/19804 (introduced
// in version 5.0), the front end can trigger a spurious expect_error failure.
template <class T> struct A {};
template <class T> struct S {
  typedef A<T*(void)> X;
  static void f();
};
struct C {
  C() { static int x = 0; }
  int i{0};
};
void f() {
  S<int>::f();
}
