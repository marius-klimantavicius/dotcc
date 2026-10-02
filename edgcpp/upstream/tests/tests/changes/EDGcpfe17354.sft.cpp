//type:fp
//options_all:--g++
//remark:[4.12] Spurious error on template-dependent compound literal in GNU C++ mode
// 7/12/16  [EDGcpfe/17354]
//
// Spurious error on template-dependent compound literal in GNU C++ mode
//
// In GNU C++ mode, the front end accepts C99-style compound literals, but it
// failed to instantiate class templates before checking for the completeness
// of the type underlying an array with unknown bounds.  This could result in
// spurious errors.
//
// This is now fixed.
template<typename T> struct X { T i; };
X<int>* x[] = { (X<int>[]){{42}} };
  // Triggered a spurious error about "X<int>[]" not being a valid
  // type for a compound literal.
