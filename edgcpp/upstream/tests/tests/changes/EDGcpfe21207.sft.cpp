//type:fp
//options_all:--gcc --gnu=90000
//remark:[6.1] GNU compatibility: __builtin_has_attribute
// 1/21/20  [EDGcpfe/21207,EDGcpfe/21623]
//
// GNU compatibility: __builtin_has_attribute
//
// The GNU __builtin_has_attribute builtin function has been implemented.  Its
// first argument is a type-or-expression (similar to sizeof) and its second
// argument is an attribute (with optional attribute arguments).
// with --gcc --gnu_version 90000:
__attribute__ ((aligned (8))) int x;
_Static_assert (__builtin_has_attribute (x, aligned), "aligned");
_Static_assert (__builtin_has_attribute (x, aligned (8)), "aligned (8)");
_Static_assert (!__builtin_has_attribute (x, aligned (4)), "aligned (4)");
