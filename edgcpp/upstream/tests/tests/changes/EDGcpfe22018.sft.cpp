//type:fp
//options_all:--c++17 --gnu_version=80300
//remark:[6.1] Abort on nontype template parameter substitution with dependent type
// 2/6/20   [EDGcpfe/22018,EDGcpfe/22131,EDGcpfe/22299]
//
// Abort on nontype template parameter substitution with dependent type
//
// In somewhat complex cases involving a member template with a nontype template
// parameter whose type depends on a previous template parameter, the front end
// aborted due to an assertion failure in get_expr_rescan_info.
//
// In this example, the problem occurred during the substitution of the type of
// the second template parameter for the constructor template of X.  This problem
// is now fixed.
template<bool> struct B;
template<typename> struct E {};
struct S { template<typename> static bool sf(); };
struct X {
  template<typename T, typename B<S::sf<T>()>::Type = true> X(E<T>);
  template<typename T> void operator=(E<T>);
};
X g();
E<int> ei;
int main() { g() = ei; }
