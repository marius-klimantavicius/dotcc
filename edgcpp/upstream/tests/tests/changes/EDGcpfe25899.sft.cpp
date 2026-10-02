//type:fp
//options_all:--c23 --clang
//remark:C23: _BitInt support
// 5/29/26  [EDGcpfe/25899,EDGcpfe/27503,EDGcpfe/28820]
//
// C23: _BitInt support
//
// In C23 mode, the front end now supports C23 _BitInt types, as specified by the
// C standardization committee's paper N2763.  A limitation of this initial
// implementation is that literals and constant-evaluated values have to fit in
// an_integer_value.
// values, _BitInt(1024) may be accepted as a type, but a _BitInt literal like
// 1234567890123456789012345678901234567890wb (which requires more than 128 bits
// to represent) will elicit an error.  In addition to C23 mode, the front end
// also accepts _BitInt types:
// In Clang C++ modes, _BitInt literal suffixes are currently not accepted (Clang
// C++ does not accept the standard suffixes either, but offers alternative
// suffixes which the current front end does not emulate).  The command-line
// option --[no_]bit_precise_integers explicitly enables or disables this feature.
typedef _BitInt(24) i24;
i24 x = 0;
