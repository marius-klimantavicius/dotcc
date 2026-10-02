//type:fp
//options_all:--c++17
//remark:[5.1] Spurious "not a literal type" error for lambda closure class
// 9/12/18  [EDGcpfe/18522,EDGcpfe/20102]
//
// Spurious "not a literal type" error for lambda closure class
//
// The front end previously incorrectly categorized some lambda closure
// classes as non-literal types when, in fact, they satisfied the requirements
// for literal class types.  This is now fixed.
constexpr auto make_lambda(int v) {
  auto ret = [=]() constexpr { return v; };  // Previously an error
  return ret;
}
