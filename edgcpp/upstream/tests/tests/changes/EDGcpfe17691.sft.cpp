//type:fp
//options_all:--c++17
//remark:[4.14] C++17: Fold Expressions
// 7/7/17   [EDGcpfe/17691,EDGcpfe/18560]
//
// C++17: Fold Expressions
//
// The front end now accepts "fold expressions", a feature that allows folding
// binary operators over parameter packs.
//
// Prototype instantiations represent this construct using a new enk_fold node
// kind, but real instantiations represent the expanded construct using existing
// IL patterns.
template<typename ... Ts> auto g(Ts ... ps) {
  return (1 * ... * ps);
}
int main() {
  return g(2, 3, 7);  // Returns 42 == 1*2*3*7
}
