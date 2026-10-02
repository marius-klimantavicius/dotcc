//type:fp
//options_all:--g++
//remark:[5.0] GNU compatibility: Complex float128/float80 types
// 10/20/17 [EDGcpfe/18231,EDGcpfe/18878,EDGcpfe/18879]
//
// GNU compatibility: Complex float128/float80 types
//
// A change has been made to accept __attribute__(mode(TC)) and
// __attribute(mode(XC)) attributes on types, thereby enabling complex __float128
// and complex __float80 types, respectively.  These types appear in some system
// header files (e.g., quadmath.h and floatn.h).  When LOWER_COMPLEX is TRUE,
// these types are lowered in a similar fashion to other complex types.
typedef _Complex float __attribute__((mode(TC))) __complex128;
typedef _Complex float __attribute__((mode(XC))) __complex80;
