//type:fp
//options_all:--c23
//remark:[6.5] C23: Binary integer literals
// 1/16/23  [EDGcpfe/25894]
//
// C23: Binary integer literals
//
// As described in WG14 paper N2549, the front end now accepts binary integer
// literals in C23 mode.
int i = 0b00001111;   // Now accepted in C23 mode
