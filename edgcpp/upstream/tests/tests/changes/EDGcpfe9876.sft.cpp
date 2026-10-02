//type:fp
//options_all:--g++
//remark:[4.1] GNU compatibility: __builtin_isnan and __builtin_isinf
// 6/8/09   [EDGcpfe/9876]
//
// GNU compatibility: __builtin_isnan and __builtin_isinf
//
// When the configuration macro TARG_HAS_IEEE_FLOATING_POINT is TRUE, the front
// end now accepts the GNU built-in functions __builtin_isnan and __builtin_isinf
// in GNU modes.  If a call to such a function has an argument that is a
// floating-point constant, the result is also a constant.
int i = __builtin_isnan(0.0/0.0);  // Same as "int i = 1;".
