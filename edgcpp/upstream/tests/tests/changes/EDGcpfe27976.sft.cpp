//type:fn
//options_all:--clang_v 999999
//remark:Abort with __is_trivially_equality_comparable applied to incomplete class
// 1/22/26  [EDGcpfe/27976,EDGcpfe/28658]
//
// Abort with __is_trivially_equality_comparable applied to incomplete class
//
// The front end previously aborted (null pointer indirection) when the type trait
// helper __is_trivially_equality_comparable is applied to an incomplete class
// type.  That is now fixed.
struct FwdDecl;
static_assert(!__is_trivially_equality_comparable(FwdDecl), "");
