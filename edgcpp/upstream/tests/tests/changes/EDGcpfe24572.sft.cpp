//type:fp
//options_all:--c11
//remark:[6.4] Out-of-range C UTF-16 character constants
// 3/29/22  [EDGcpfe/24572]
//
// Out-of-range C UTF-16 character constants
//
// In the C11 and later standards, a UTF-16 character constant that cannot be
// represented in a single 16-bit code unit has an implementation-defined
// value, typically the low-order 16 bits.  The front end, however, previously
// followed the C++ rules, in which such a character literal is an error.
// This is now fixed.
int c = u'\x1f680';  // Previously an error, now okay
