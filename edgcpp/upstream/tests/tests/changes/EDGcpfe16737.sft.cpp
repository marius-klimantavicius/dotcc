//type:fp
//options_all:--g++ --c++11
//remark:[4.11] GNU Compatibility: Allow attributes on lambda before exception specification
// 1/17/16  [EDGcpfe/16737,EDGcpfe/16774]
//
// GNU Compatibility: Allow attributes on lambda before exception specification
//
// A change has been made to allow attributes on a lambda before an exception
// specification.  In addition, GNU attributes (but not standard attributes) are
// allowed in that location in clang emulation mode.
// --c++11):
auto f = [] () __attribute__((noinline)) noexcept { };
