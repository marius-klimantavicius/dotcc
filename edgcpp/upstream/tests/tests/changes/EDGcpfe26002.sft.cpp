//type:fp
//options_all:--gn 120100 --c11
//remark:[6.5] _Atomic as an array qualifier
// 4/21/23  [EDGcpfe/26002,EDGcpfe/26265]
//
// _Atomic as an array qualifier
//
// In C11 mode, the changes for EDGcpfe/25245 introduced a regression in version
// 6.4 of the front end, causing it to no longer accept _Atomic as a C-style array
// qualifier.
//
// That is now fixed.
extern void f (int [_Atomic]);  // Now accepted by version 6.4 in C11 mode.
