//type:fp
//options_all:--c++11
//remark:[5.0] User-defined literals appearing in template arguments
// 10/23/17 [EDGcpfe/18711]
//
// User-defined literals appearing in template arguments
//
// The front end previously failed in some cases to find the correct literal
// operator for a user-defined literal appearing in a template argument,
// either issuing a spurious "not found" diagnostic or silently choosing an
// incorrect literal operator.  This is now fixed.
namespace N {
  constexpr int operator"" _c(unsigned long long val) {
    return val;
  }
}
template <typename T, T VAL> struct A {
  static constexpr int value = VAL;
};
void f() {
  using namespace N;
  A<int, 25_c> a;  // Previously literal operator not found, now okay
}
