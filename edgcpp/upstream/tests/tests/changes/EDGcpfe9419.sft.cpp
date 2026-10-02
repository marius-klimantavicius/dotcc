//type:fp
//options_all:--g++
//remark:[4.1] GNU compatibility: Built-in bit counting functions
// 12/17/08 [EDGcpfe/9419]
//
// GNU compatibility: Built-in bit counting functions
//
// In GNU modes, the front end predeclares a number of bit counting functions:
// __builtin_ffs, __builtin_clz, __builtin_ctz, __builtin_popcount,
// __builtin_parity, and variants of all these for unsigned long and unsigned
// long long arguments.  Now, calls to these routines are folded to constants
// if the argument is a constant integer whose value can be represented by
// a_host_large_unsigned.
char x[__builtin_popcount(0xFF00)];  
  // Now accepted in GNU modes. Same as "char x[8];" since popcount
  // counts the number of "one bits" in its argument.
