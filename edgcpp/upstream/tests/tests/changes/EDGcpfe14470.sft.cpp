//type:fp
//remark:[4.8] Spurious suppression of default nested class constructor
// 9/10/13  [EDGcpfe/14470]
//
// Spurious suppression of default nested class constructor
//
// In C++11 and Microsoft C++ modes, the front end sometimes incorrectly
// suppressed the default constructor of a nested class that requires a call
// to another nested class default constructor involving a default argument.
//
// This is now fixed.
struct S {
  struct D { D(int = 0); };
  struct N { D d; };  // Default constructor was accidentally suppressed
};                    // in some modes.
S::N n;  // Previously a spurious error.
