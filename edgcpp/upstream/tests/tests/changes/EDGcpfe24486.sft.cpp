//type:fp
//options_all:-W --c++17
//remark:[6.3] Spurious "no effect" warning on fold-expression
// 7/29/21  [EDGcpfe/24486]
//
// Spurious "no effect" warning on fold-expression
//
// The front end previously emitted spurious "expression has no effect" warnings
// for some fold expressions.
//
// The "no effect" warning was emitted for this example even though an overloaded
// operator<< used for the substituted fold-expression could well have side
// effects. That is now fixed (i.e., the warning is no longer emitted in these
// cases).
struct S {} s;
template<typename... Ts> void g(Ts ...args) {
  (s << ... << args);  // Previously a spurious warning.
}
