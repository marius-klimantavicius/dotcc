//type:fp
//options_all:--c++23
//remark:[6.8] C++23: initializing character arrays with UTF-8 literals
// 3/20/25  [EDGcpfe/25499,EDGcpfe/27995]
//
// C++23: initializing character arrays with UTF-8 literals
//
// WG21 document P2513R3 changed the rules for initializing an array of char
// or unsigned char so that a UTF-8 string literal is an acceptable
// initializer.  The front end has now been updated accordingly.  Because the
// paper was adopted as a defect report, this change applies in all modes that
// support UTF-8 string literals.
char s[] = u8"abc";   // Previously an error, now okay
