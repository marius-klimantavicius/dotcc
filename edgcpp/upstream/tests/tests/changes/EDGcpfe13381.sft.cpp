//type:fp
//options_all:--g++
//remark:[4.6] GNU C++ compatibility: Typedefs for bool in system headers
// 11/16/12 [EDGcpfe/13381]
//
// GNU C++ compatibility: Typedefs for bool in system headers
//
// In GNU C++ mode, the front end now ignores typedefs for bool that appear
// in system headers.
//
// (See the entry for EDGcpfe/12755 on 3/13/12 for the similar case of a
// typedef for wchar_t.)
#2 "t.c" 3
typedef int bool;  // Now silently accepted in GNU C++ mode.
