//type:fp
//remark:[4.0] Abort on reference-to-reference-to-function construct
// 9/12/08  [EDGcpfe/9221]
//
// Abort on reference-to-reference-to-function construct
//
// Attempting to add a type qualifier (like "const") to a function type through
// a reference-to-reference construct resulted in an internal error (in
// make_qualified_type, il.c).
//
// This is now fixed: The type qualifier is ignored (with a warning).
void f() {
  typedef void (&RF)();
  RF const &rf = f;  // Previously triggered an internal error.
}
