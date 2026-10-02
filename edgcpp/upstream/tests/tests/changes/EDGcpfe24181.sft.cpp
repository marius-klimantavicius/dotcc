//type:fp
//options_all:--c++11
//remark:[6.8] Overload resolution failure for parameters declared using typedef names
// 7/28/25  [EDGcpfe/24181]
//
// Overload resolution failure for parameters declared using typedef names
//
// Previously, the front end did not always treat a typedef name as
// equivalent to the aliased type during partial ordering, which could lead to
// spurious overload resolution failures.
using INT = int;
template<typename T> int f(T, INT &&);
template<typename T> int f(T *, int &&);
int i = f("", 0);  // Previously a spurious error.  Now okay.
