//type:fp
//options_all:--c++20
//remark:[6.1] Abort on attempt to substitute conditional explicit specifier
// 5/4/20   [EDGcpfe/22471]
//
// Abort on attempt to substitute conditional explicit specifier
//
// In C++20 mode, the front end supports "conditional explicit" specifiers (see
// the entry for EDGcpfe/20042 of 10/9/18).  In some cases, such a specifier
// requires substitution, but the front end sometimes failed to record the
// auxiliary data needed to perform such a substitution, which caused it to
// abort with an internal error in get_expr_rescan_info later on.
//
// That problem is now fixed.
template<typename T, int N = -1> struct S {
  explicit(N != -1) S(T*);
};
S s = (int*)0;  // Previously aborted.  Now okay.
