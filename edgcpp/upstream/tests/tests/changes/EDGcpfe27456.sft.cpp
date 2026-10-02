//type:fp
//options_all:--c++26
//remark:[6.7] C++26: constexpr structured bindings
// 7/23/24  [EDGcpfe/27456]
//
// C++26: constexpr structured bindings
//
// In C++26 mode, the front end now accepts constexpr structured bindings,
// including the case of bindings for tuple-like types.  That support includes
// allowing constexpr references to local variables.
//
// This implements the proposal in the standardization committee's paper P2686R4.
// Since that paper has not been voted into the draft for the next standard, it
// is possible that the feature will not actually make it in C++26 (although the
// paper has progressed through most of the standardization process and is
// expected to be approved in the relatively near future).
void f() {
  constexpr int i = 42;
  constexpr int const &r = i;  // Now accepted in C++26 mode.
  static_assert(r == 42);
}
