//type:fp
//options_all:--gnu_version=90300 --c++20 --no_warnings -tused
//remark:[6.7] Memory region violation with nontype template argument of class type
// 12/17/24 [EDGcpfe/27697]
//
// Memory region violation with nontype template argument of class type
//
// In some configurations, this example triggered an abort in the front end (e.g.,
// while reading the corresponding IL file) due to a violation of the rule that
// file-scope IL entries (in this case, the template argument in f<s>) cannot
// contain pointers to function-scope IL entries (in this case, the a_variable
// entry representing s).  That is now fixed.
struct S {};
template<S> void f() {}
int main() {
  constexpr S s;
  f<s>();
}
