//type:fp
//options_all:--strict --c23
//remark:[6.5] C23: ellipsis-only variadic functions
// 1/17/23  [EDGcpfe/25945]
//
// C23: ellipsis-only variadic functions
//
// As described in WG14 paper N2975, the front end now accepts in C23 mode
// function declarations in which the parameter list consists solely of an
// ellipsis.
void f(...) { }   // Now accepted in C23 mode
