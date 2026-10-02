//type:fp
//remark:[4.10.1] Floating-point bound in array new-expression
// 3/2/15   [EDGcpfe/16045]
//
// Floating-point bound in array new-expression
//
// The first array bound in a new-expression (the one that can be non-constant)
// can now have a floating-point type in Microsoft C++ mode and in C++14 modes.
//
// The C++14 change is a result of a language change introduced by the C++
// standards committee's paper N3323.
int *p = new int[1.0]; // Now accepted in C++14 and Microsoft modes.
