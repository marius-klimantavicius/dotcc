//type:fp
//options_all:--gcc --c11
//remark:[6.5] Spurious error on _Alignas
// 12/19/22 [EDGcpfe/21220]
//
// Spurious error on _Alignas
//
// In some cases where _Alignas followed an attribute, a spurious error had been
// given.
const __attribute__((used)) _Alignas(4) int a = 1;
