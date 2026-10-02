//type:fp
//options_all:--c++14 --g++
//remark:[4.12] GNU compatibility: decltype(auto) and type qualifiers
// 9/15/16  [EDGcpfe/17395]
//
// GNU compatibility: decltype(auto) and type qualifiers
//
// Ordinarily, a decltype(auto) specifier in a variable declaration cannot have
// added type qualifiers.  GCC, however, does accept them and we now emulate that
// behavior in GNU C++14 mode.
decltype(auto) const x = 3;  // Now accepted in GNU C++14 mode.
