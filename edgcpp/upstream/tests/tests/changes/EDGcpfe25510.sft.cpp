//type:fp
//options_all:--c++23
//remark:[6.7] C++23: Delimited escape sequences
// 6/20/24  [EDGcpfe/25510]
//
// C++23: Delimited escape sequences
//
// C++ Committee paper P2290R3 added delimited escape sequences for octal
// (\o) and hexadecimal (\x) numeric escapes and universal-character-names
// (\u).  The front end now accepts these escape sequences in C++23 mode and
// in GNU mode (when gnu_version is at least 130000) and clang_mode (when
// clang_version is at least 140000).
const char8_t *u = u8"\u{a01}";
const char *o = "\o{123}";
const char *x = "\x{ab}";
