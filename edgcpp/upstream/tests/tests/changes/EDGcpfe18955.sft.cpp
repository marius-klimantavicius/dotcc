//type:fp
//options_all:--c++11
//remark:[6.7] Spurious error on template keyword following global namespace qualifier
// 8/26/24  [EDGcpfe/18955,EDGcpfe/27415]
//
// Spurious error on template keyword following global namespace qualifier
//
// Previously, the front end issued a spurious error when the template keyword
// followed the global namespace qualifier.
template<typename>
struct S {};
::template S<void> s;  // Previously a spurious error.  Now okay.
