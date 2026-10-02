//type:fp
//options_all:--gcc
//remark:[4.13] GCC compatibility: warning on redeclaration of builtin function
// 10/24/16 [EDGcpfe/17670]
//
// GCC compatibility: warning on redeclaration of builtin function
//
// Previously, a redeclaration of a builtin function with a different function
// signature in gcc emulation mode triggered an error; now a warning is given.
void _Exit() {}
