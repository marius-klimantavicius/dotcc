//type:fp
//options_all:--c++14
//remark:[5.0] Internal error in find_subobject_for_interpreter_address
// 10/19/17 [EDGcpfe/18840]
//
// Internal error in find_subobject_for_interpreter_address
//
// In some cases, when constexpr evaluation produces the address one past the end
// of an array member inside a base class subobject, the front end aborted with an
// internal error in find_subobject_for_interpreter_address.
//
// Previously, the evaluation of "d.cend()" produced an address that the constexpr
// interpreter failed to translate back to a target representation (resulting in
// the internal error).  That is now fixed.
struct B {
  float s[2];
  constexpr B(): s{ 1.0f, 2.0f } {}
  constexpr auto cbegin() const { return this->s; }
  constexpr auto cend() const { return this->s+2; }
};
struct D: public B {
  constexpr D() : B{} {}
};
constexpr float g() {
  constexpr D d;
  for (auto it = d.cbegin(); it < d.cend(); ++it) {}  // Previously aborted.
  return 0.0;                                         // Now okay.
}
