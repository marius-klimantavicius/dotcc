//type:fp
//options_all:--c++17
//remark:[6.5] Instantiation of discarded substatement in constexpr if
// 12/20/22 [EDGcpfe/20142,EDGcpfe/21012,EDGcpfe/25683]
//
// Instantiation of discarded substatement in constexpr if
//
// During the instantiation of an enclosing templated entity, the front end
// incorrectly instantiated the discarded substatement of a "constexpr if" with a
// non-dependent condition.
template<bool B, class R, class T> R f(T t) {
  return [] (auto v) {
    if constexpr (B) return T::v;  // Previously a spurious error.  Now okay.
  } (t);
}
template void f<false>(int);
