//type:fp
//options_all:--clang
// 8/28/26  [EDGcpfe/24003,EDGcpfe/26323,EDGcpfe/29024]
//
// Clang compatibility: two argument form of "deprecated" attribute
//
// The front end now accepts the two-argument form of the "deprecated" attribute
// in Clang emulation modes.
void f(void) __attribute__((deprecated("message", "replacement")));
