//type:fn
//options_all:--c11 --gcc
//remark:[4.13] Segmentation violation on invalid use of _Noreturn
// 10/19/16 [EDGcpfe/17651]
//
// Segmentation violation on invalid use of _Noreturn
//
// As a result of the changes for EDGcpfe/17383, a segmentation violation (in
// apply_c11_noreturn) had occurred on invalid uses of the C11 _Noreturn function
// specifier in GNU emulation mode.  Now fixed.
typedef _Noreturn void f(void);
