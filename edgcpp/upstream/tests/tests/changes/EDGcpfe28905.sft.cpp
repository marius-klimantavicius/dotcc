//type:fn
//options_all:--microsoft
//remark:Microsoft mode abort on incomplete struct typedef
// 6/22/26  [EDGcpfe/28905]
//
// Microsoft mode abort on incomplete struct typedef
//
// Previously, this aborted with an internal error in pop_scope_full
// (scope_stk.c).  That is now fixed.
typedef struct {
