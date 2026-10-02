//type:fp
//options_all:-w --gnu_version 79999
//remark:[6.0] IL write-read abort after folding local variable initializer
// 11/12/19 [EDGcpfe/21949]
//
// IL write-read abort after folding local variable initializer
//
// In some (but not all) configurations with IL_SHOULD_BE_WRITTEN_TO_FILE,
// folding a local variable initializer involving a constexpr call occasionally
// resulted in an IL write-read abort.  This was caused by the dynamic initializer
// entry of the corresponding stmk_init statement not being updated to reflect the
// folded initializer.
//
// Sometimes, the abort did not happen in the front end proper, but in standalone
// utilities, like the IL display program.  That problem is now fixed.
enum E { e };
constexpr E f() { return e; }
template<typename> struct A {
  A() {
    const E g = f();  // Previously resulted in inconsistent IL, leading
  }                   // to an IL write-read abort.
};
struct B {
  A<char> a;
  B(): a() {}
};
