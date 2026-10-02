//type:fp
//options_all:--g++
//remark:[6.1] Address of parenthesized id-expression as template argument
// 12/5/19  [EDGcpfe/22076]
//
// Address of parenthesized id-expression as template argument
//
// The changes for EDGcpfe/20944 (version 6.0) introduced a regression in
// pre-C++17 modes, causing the front end to erroneously reject the address of a
// parenthesized id-expression as a template argument.
//
// That is now fixed.
template<int*> struct S {};
int i = 42;
S<&(i)> sai = {};  // Error in version 6.0 in pre-C++17 modes.  Now okay.
