//type:fp
//options_all:--microsoft_v 1928
//remark:[6.2] Abort on constant-evaluation of Microsoft-mode prvalue conditional expression
// 11/19/20 [EDGcpfe/23605]
//
// Abort on constant-evaluation of Microsoft-mode prvalue conditional expression
//
// The constexpr interpreter previously sometimes aborted (in do_constexpr_ctor)
// when attempting to evaluate a conditional expression producing a prvalue for a
// nontrivial class type.
//
// That is now fixed.
struct D { char data[4]; };
struct X {
  constexpr X() = default;
  constexpr X(X&&) {}
  constexpr X(X const &x): val{x.val} {}
  D val{};
};
constexpr X f() {
  X v{};
  return true ? v : X{};
}
void g() { f(); }  // Previously triggered an abort in Microsoft C++ mode.
                   // Now okay.
