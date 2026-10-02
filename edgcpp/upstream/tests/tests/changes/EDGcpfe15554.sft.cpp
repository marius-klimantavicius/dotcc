//type:fp
//options_all:--c++11
//remark:[4.10] Incorrect handling of numeric literal suffixes with user-defined literals
// 12/18/14 [EDGcpfe/15554]
//
// Incorrect handling of numeric literal suffixes with user-defined literals
//
// The front end previously incorrectly treated suffixes such as U or L
// appearing in a numeric user-defined literal as belonging to the numeric
// portion rather than as part of the literal suffix.  A similar problem
// occurred in a floating-point user-defined literal where the literal suffix
// begins with "e" or "E", which could be mistaken as the start of the
// exponent.  These are now fixed.
void operator""LL(long double);
void operator""exa(long double);
void f() {
  1.2LL;  // Previously an error, "user-defined literal operator not found"
  1.2exa; // Previously an error, "invalid floating constant"
}
