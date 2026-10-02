//type:fp
//options_all:--gnu_version 60000 -tused
//remark:[5.0] Spurious errors on pack expansions in SFINAE contexts
// 8/7/18   [EDGcpfe/18914]
//
// Spurious errors on pack expansions in SFINAE contexts
//
// When processing pack expansions for expression lists in SFINAE contexts, the
// front end previously sometimes lost track of the pack pattern to expand when
// dealing with multiple cascading expansions.
//
// Here the instantiation of R<C<int>>::operator() previously produced a spurious
// error.  That is now fixed.
struct A { void operator ()(...) const; };
template<typename T> struct C {
  template<int ... I>
    auto f() -> decltype(A()(I == T()...));  // Pack expansion.
  template<typename ... Args>
    auto operator()() -> decltype(f<0>(Args()...));  // Pack expansion.
};
template<typename F> struct R {
  auto operator ()() -> decltype(F()());  // Previously a spurious error.
};
template<typename F> R<C<int>> g(const F &f...);
int main() {
  g(A());
}
