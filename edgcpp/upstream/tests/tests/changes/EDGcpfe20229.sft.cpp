//type:fp
//options_all:--c++14
//remark:[5.1] Invalid scanning of single digit followed by backslash or extended character
// 10/10/18 [EDGcpfe/20229]
//
// Invalid scanning of single digit followed by backslash or extended character
//
// The changes for EDGcpfe/18278 (which optimize single-digit number tokens)
// introduced a regression (in version 4.14) in some cases where a single digit
// is followed by a universal character name or extended character.
//
// That is now fixed.
#define M(discard, ...) __VA_ARGS__
int r = M(0\u1234'0, 42);  // Previously triggered spurious errors in
                           // C++14 mode.
