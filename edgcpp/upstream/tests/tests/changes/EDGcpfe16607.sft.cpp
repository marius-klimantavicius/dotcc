//type:fp
//options_all:--microsoft
//remark:[4.11] Microsoft C++ compatibility: Same-type casts in decltype constructs
// 11/3/15  [EDGcpfe/16607]
//
// Microsoft C++ compatibility: Same-type casts in decltype constructs
//
// In Microsoft mode, the front end usually ignores a cast of an lvalue to the
// type of that lvalue.  Ordinarily, such a cast would produce an rvalue, but by
// ignoring the cast the resulting expression remains an lvalue.  Now, the front
// end no longer ignores the cast inside a decltype operand: That ensures that a
// non-reference type is produced if such a cast appears at the top-level of a
// decltype operand.
//
// Previously, "decltype((int)i)" was treated as "decltype(i)" and therefore
// produced an int& type (resulting in an error since the reference has no
// initializer).  Now, the cast is not ignored and "decltype((int)i)" produces
// an int type (which is standard behavior).
void g(int i) {
  (int)i = 1;          // Accepted in Microsoft C++ mode.
  decltype((int)i) c;  // Previously an error; now okay.
}
