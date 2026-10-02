//type:fp
//options_all:--g++
//remark:[4.1] GNU compatibility: __real/__imag and integral arguments
// 1/15/09  [EDGcpfe/9441]
//
// GNU compatibility: __real/__imag and integral arguments
//
// In GNU C and C++ mode, the front end now accepts __real and __imag operators
// applied to arguments of integral (and enumeration) types.  A warning is issued
// in such cases.  As with arguments of real floating point types, __real applied
// to an integral operand leaves the operand unchanged, and __imag applied to an
// integral operand produces a zero of the same type as the operand.
long i = __imag(3L);  // Accepted in GNU modes.  Same as "long i = 0L;".
