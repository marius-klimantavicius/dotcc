//type:fp
//options_all:--gn 80400 --c++17
//remark:[6.2] Deducing a boolean template parameter from a noexcept property
// 9/9/20   [EDGcpfe/23367]
//
// Deducing a boolean template parameter from a noexcept property
//
// In Clang and GNU C++ modes, the front end supports the deduction of a boolean
// template parameter based on whether a function type is "noexcept" or not: See
// the entry for EDGcpfe/18317.  However, the emulation of that extension had a
// bug that caused "noexcept" (with no explicitly-specified value) to be treated
// as "noexcept(false)" for deduction purposes, when it really should be treated
// as "noexcept(true)".
//
// That problem is now fixed.
template<typename R, typename ...Ps, bool N>
  constexpr bool is_noexcept(R(&)(Ps...) noexcept(N)) { return N; }
void g() noexcept;
static_assert(is_noexcept(g));  // Previously failed.  Now okay.
