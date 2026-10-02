//type:fp
//options_all:--c23
//remark:[6.7] C23: nullptr keyword and nullptr_t type
// 7/11/24  [EDGcpfe/25950,EDGcpfe/26287,EDGcpfe/27445]
//
// C23: nullptr keyword and nullptr_t type
//
// The nullptr keyword and nullptr_t type, as described in C Committee
// document N3042, are now enabled in C23 mode.  In addition, the nullptr
// keyword (but not the nullptr_t keyword, to avoid collisions with
// user-declared identifiers) is enabled in pre-C23 modes when the --nullptr
// command-line option is specified (it was previously disallowed in C
// language modes).
void *p = nullptr;
void f(nullptr_t);
