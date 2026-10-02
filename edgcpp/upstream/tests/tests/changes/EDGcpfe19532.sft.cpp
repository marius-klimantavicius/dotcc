//type:fp
//options_all:--clang --c
//remark:[5.1] clang compatibility: C overloadable functions with ellipsis and no named
// 12/13/18 [EDGcpfe/19532]
//
// clang compatibility: C overloadable functions with ellipsis and no named
// parameters
//
// In C mode, clang allows ellipsis and no named parameters in a function
// that has an __overloadable__ attribute.
//
// The front end now accepts this in clang C mode.
void p(...)  __attribute__((__overloadable__));
