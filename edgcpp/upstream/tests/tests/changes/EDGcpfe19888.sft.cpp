//type:fp
//options_all:--microsoft_v 1914
//remark:[5.1] Abort on use of deleted function with deducible return type
// 8/21/18  [EDGcpfe/19888]
//
// Abort on use of deleted function with deducible return type
//
// Some cases of deleted function declarations with a deducible return type
// resulted in an internal error (in check_defaulted_or_deleted_function in
// class_decl.c).
//
// This is now fixed.  (See also EDGcpfe/18910, which resolved a variation of
// this issue.)
struct S {
  auto& f() = delete;  // Previously aborted.  Now okay.
};
