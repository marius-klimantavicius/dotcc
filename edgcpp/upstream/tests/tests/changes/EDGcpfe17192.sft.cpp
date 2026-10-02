//type:fp
//options_all:--c++14
//remark:[4.12] Abort on constexpr call producing an address in a base class subobject
// 7/21/16  [EDGcpfe/17192,EDGcpfe/17423]
//
// Abort on constexpr call producing an address in a base class subobject
//
// In C++14 mode, the front end sometimes aborted with an internal error in
// find_subobject_for_interpreter_address (interpret.c) processing a call
// returning an address (pointer or reference) into a base class subobject.
//
// This is now fixed.
struct B1 { int i; };
struct B2 { int i; };
struct D: B1, B2 {
  constexpr D(): B1{1}, B2{2} {}
  constexpr int const& f() const { return B2::i; };
};
int main() {
  constexpr D d;
  return d.f();  // d.f() returns a reference in the base class subobject
}                // of d.  Previously aborted.  Now okay.
