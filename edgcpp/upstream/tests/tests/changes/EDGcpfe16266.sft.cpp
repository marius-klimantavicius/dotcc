//type:fp
//options_all:--c++14
//remark:[5.1] Spurious error on attribute in alias declaration
// 12/19/18 [EDGcpfe/16266]
//
// Spurious error on attribute in alias declaration
//
// A spurious error had been given when an alias declaration contains an
// attribute.
using x [[deprecated]] = int;
