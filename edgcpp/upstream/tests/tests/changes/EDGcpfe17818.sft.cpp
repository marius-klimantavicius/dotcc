//type:fp
//options_all:--microsoft
//remark:[4.13] Microsoft compatibility: Address of dllimport variable
// 12/14/16 [EDGcpfe/17818]
//
// Microsoft compatibility: Address of dllimport variable
//
// In Microsoft C++ mode, the address of a dllimport variable can now appear in
// any constant-expression context (previously, the only constant-expression
// contexts where this was accepted were template arguments).
__declspec(dllimport) int v;
constexpr int *pv = &v;  // Previously an error.  Now okay.
