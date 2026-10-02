//type:fp
//options_all:--microsoft
//remark:[4.14] Microsoft compatibility: hexadecimal literals and preprocessing numbers
// 2/24/17  [EDGcpfe/17935]
//
// Microsoft compatibility: hexadecimal literals and preprocessing numbers
//
// According to the C++ Standard, a construct like 0xE+0x1 is a single
// preprocessing number that is ill-formed (i.e., requires an error diagnostic)
// because it cannot be converted to a numeric literal.  MSVC accepts such
// constructs, however, treating the "+" character as terminating the
// hexadecimal literal 0xE.  The front end has now been changed to do the same
// in Microsoft mode.
int i = 0xE+0x1;   // Now equivalent to 15 (0xE + 0x1) in Microsoft mode
