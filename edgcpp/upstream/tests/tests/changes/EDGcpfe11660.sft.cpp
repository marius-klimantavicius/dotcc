//type:fn
//options_all:--microsoft
//remark:[4.4] Abort on attempt to declare a friend constructor in Microsoft mode
// 11/22/11 [EDGcpfe/11660]
//
// Abort on attempt to declare a friend constructor in Microsoft mode
//
// In Microsoft mode, trying to declare a friend constructor with an elaborated
// unqualified name (which is permissible in Microsoft mode for ordinary
// constructor declarations) resulted in an abort in f_identical_types (trying
// to dereference a NULL type pointer).
//
// This is now fixed.
struct S {
  friend struct S(S*);
};
