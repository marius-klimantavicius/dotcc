//type:fn
//options_all:--c11
//remark:[5.1] _Alignof/alignof operator
// 4/12/19  [EDGcpfe/21128]
//
// _Alignof/alignof operator
//
// The front end now issues a discretionary error when applying the C++11 alignof
// operator or the C11 _Alignof operator to a function type (except in GNU modes).
// In C11 mode, a diagnostic is now also issued on _Alignof applied to an array
// type with an unspecified bound (an error in strict C11 mode, a warning
// otherwise).
//
// Note that the behavior using nonstandard syntax (e.g., "__ALIGNOF__") is not
// affected by this change.
long a1 = _Alignof(int(void));  // Now an error in C11 mode.
long a2 = _Alignof(int[]);      // Now a warning in nonstrict C11 mode,
                                // and an error in strict C11 mode.
