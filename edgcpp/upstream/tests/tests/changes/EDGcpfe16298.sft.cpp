//type:fp
//remark:[4.11] C++-generating back end abort on function declared with decltype
// 10/15/15 [EDGcpfe/16298]
//
// C++-generating back end abort on function declared with decltype
//
// When a function is first declared with a type expressed via a decltype
// specifier, and later defined using an ordinary function declarator, the front
// end incorrectly recorded the "declared type" of the function definition as
// the type entry representing the decltype construct.  If the latter construct
// appears in a local scope, this could trigger an internal error in the
// C++-generating back end (gen_routine_specifiers_and_declaration).
//
// This is now fixed.
void f() {}
void h() {
  decltype (f) g;
}
void g() {}  // Rendering of this definition triggered an abort in the
             // C++-generating back end.
