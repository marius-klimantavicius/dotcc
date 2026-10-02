//type:fp
//options_all:--strict --c23
//remark:[6.5] C23: value of __STDC_VERSION__
// 3/27/23  [EDGcpfe/26183]
//
// C23: value of __STDC_VERSION__
//
// In anticipation of the adoption of the next version of the ISO C Standard,
// the front end now defines the macro __STDC_VERSION__ to have the value
// 202311L in C23 mode.
int i;
static_assert(__STDC_VERSION__ == 202311L, "Bad version number.");
