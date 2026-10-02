//type:fp
//options_all:--c11 --gnu=40700
//remark:[4.12] GNU compatibility: Multiple _Alignas alignment specifiers
// 8/15/16  [EDGcpfe/17388]
//
// GNU compatibility: Multiple _Alignas alignment specifiers
//
// The C11 standard dictates that when multiple _Alignas alignment specifiers
// are present, the strictest alignment is used.  That had been true except when
// gnu_version < 40800.  Now fixed.
long _Alignas(4) _Alignas(8) _Alignas(1) x;
_Static_assert(__alignof(x) == 8, "");
