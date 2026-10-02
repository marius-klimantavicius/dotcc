//type:fp
//options_all:--c++17
//remark:[6.0] Narrowing conversion in deduction contexts
// 9/19/19  [EDGcpfe/21624]
//
// Narrowing conversion in deduction contexts
//
// The front end did not always correctly treat invalid narrowing conversions as
// deduction failures.
//
// That problem is now fixed.
template<int (*p)()>
  constexpr bool narrows(decltype(int{(p(), 0U)})) { return true; }
                                        // Previously a spurious warning.
template<int (*p)()>
  constexpr bool narrows(...) { return false; }
int g() { return 0; }
static_assert(!narrows<g>(0));  // Previously an error.  Now okay.
