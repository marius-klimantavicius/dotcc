//type:fp
//options_all:--c++14
//remark:[6.7] Spurious error on pack expansion in nested generic lambda
// 5/8/24   [EDGcpfe/27216]
//
// Spurious error on pack expansion in nested generic lambda
//
// A generic lambda containing a pack expansion that references packs from both an
// enclosing real instantiation and the generic lambda would previously elicit a
// spurious error.
int g(int, int);
template<typename ... Ts>
inline int f(Ts ... t) {
  return [] (auto ... p) {
    return g(Ts{p} ...);  // Previously a spurious error.  Now okay.
  } (t ...);
}
auto i = f(1, 2);
