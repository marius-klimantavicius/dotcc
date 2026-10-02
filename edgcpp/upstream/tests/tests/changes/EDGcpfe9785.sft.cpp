//type:fn
//remark:[4.1] Pointer-to-reference-member constants
// 5/28/09  [EDGcpfe/9785]
//
// Pointer-to-reference-member constants
//
// The front end previously failed to diagnose pointer-to-member constants that
// referred to reference members.  In version 4.0 (but not in prior versions)
// this often resulted in an internal error in scan_ptr_to_member_operator
// (expr.c) when the constant was used in any way.
//
// Similar problems could occur when such pointer-to-member constants appear in
// the deduction of template parameters or auto type specifiers.  This is now
// fixed.
struct S { S(); int &x; };
int main() {
  S().*(&S::x);  // Previously resulted in an internal error.  Now, a
}                // diagnostic is emitted on "&S::x".
