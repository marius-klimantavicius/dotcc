//type:fp
//options_all:--c++11
//remark:[5.0] SFINAE involving implicit "this" selector
// 9/19/17  [EDGcpfe/18665]
//
// SFINAE involving implicit "this" selector
//
// In some cases, the front end failed to resolve a call appearing in a function
// signature during SFINAE processing if the call relied on an implicit "this"
// member selector.
//
// Previously, the call to operator() failed because the call f(ps...) in the
// return type of that operator was not correctly handled (specifically, that
// call involves an implicit "this" selector, which was incorrectly ignored by
// the front end).  That is now fixed.
struct S1 {};
void g1(S1) {}
struct S2 {};
void g2(S2) {}
struct X {
  template <typename... Ts> auto f(Ts... ps) -> decltype(g1(ps...)) {
    return g1(ps...);
  }
  template <class... Ts> auto f(Ts... ps) -> decltype(g2(ps...)) {
    return g2(ps...);
  }
  template <class... Ts> auto operator()(Ts... ps) -> decltype(f(ps...)) {
    return f(ps...);
  }
};
int main() {
  X()(S2{});  // Previously found no match.  Now okay.
}
