//type:fp
//options_all:--c11
//remark:[4.9] C11: _Alignof and _Alignas
// 1/22/14  [EDGcpfe/12779]
//
// C11: _Alignof and _Alignas
//
// In C11 mode, the front end now accepts the _Alignas(...) construct as well as
// _Alignof as a synonym for __alignof (although in strict mode, only a type
// operand is permitted for _Alignof).
//
// Note that this is entirely similar to the C++11 "alignas" and "alignof"
// features, and this is true also for the underlying implementation.  In
// particular, the "_Alignas" construct is represented internally as a kind of
// attribute (even in C11 modes that do not support a general attribute
// mechanism).
_Alignas(int) char buf[100];
int buf_alignment = _Alignof(int);
