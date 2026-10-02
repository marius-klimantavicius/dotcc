//type:fp
//options_all:--c++11
//remark:[4.13] Spurious error on non-evaluated access to variable in C++11 mode
// 12/2/16  [EDGcpfe/17643]
//
// Spurious error on non-evaluated access to variable in C++11 mode
//
// In C++11, a built-in logical operator (&& or ||) with a constant first operand
// that short-circuits the evaluation of the second operand is a valid constant-
// expression even if the second operand is not (that is not the case in C++03).
// The front end, however, sometimes issued a spurious error in such cases.
//
// That is now fixed.
struct U { bool operator&& (U const& rhs); };
bool b;
double x[1+(0 && b)];  // Previously triggered an error in C++11 mode
                       // because b is nonconstant.  Now okay.
