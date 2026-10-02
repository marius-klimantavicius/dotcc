//type:fn
//options_all:--c++20
//remark:[6.1] Diagnose invalid attributes in more cases
// 1/14/20  [EDGcpfe/20839]
//
// Diagnose invalid attributes in more cases
//
// Changes have been made to diagnose invalid attributes in more cases.
typedef void (*fp)([[no_unique_address]] void);     // Now an error.
template <int I [[no_unique_address]]> struct A {}; // Now an error.
