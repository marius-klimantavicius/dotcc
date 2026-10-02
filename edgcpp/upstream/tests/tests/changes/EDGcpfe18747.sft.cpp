//type:fp
//options_all:--c++17
//remark:[5.0] C++17: Nontype template arguments
// 9/15/17  [EDGcpfe/18747]
//
// C++17: Nontype template arguments
//
// The forthcoming C++17 standard generalizes the permitted forms of nontype
// template arguments slightly, particularly when it comes to nontype template
// parameters of pointer, reference, or pointer-to-member type.  The front end
// now enables the new rules in C++17 mode.
//
// These changes were added to the working paper for the C++17 standard through
// paper N4268.
template<int*> struct X {};
struct S { static int m; } s;
X<&s.m> x;  // Now okay in C++17 mode.  (Previously an error.)
