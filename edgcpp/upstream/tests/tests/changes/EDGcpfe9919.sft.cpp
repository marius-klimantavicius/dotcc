//type:fp
//options_all:--g++
//remark:[4.4] GNU compatibility: __builtin_isfinite and __builtin_isnormal
// 6/17/11 [EDGcpfe/9919,EDGcpfe/11661]
//
// GNU compatibility: __builtin_isfinite and __builtin_isnormal
//
// In GNU modes (in configurations that have TARG_HAS_IEEE_FLOATING_POINT set to
// TRUE) the front end now supports the GNU built-in functions __builtin_isfinite
// and __builtin_isnormal.  A call to one of these functions with a constant
// floating-point argument is folded in the front end if the internal format of
// floating-point constant permits it.
int i = __builtin_isnormal(1e-40F);  // Now equivalent to "int i = 0;" in
                                     // GNU C mode.
