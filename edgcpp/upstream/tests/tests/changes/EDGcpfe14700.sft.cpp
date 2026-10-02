//type:fp
//options_all:--c++11
//remark:[4.10] Incorrect IL for lowered thread_local rvalue references
// 12/2/14  [EDGcpfe/14700]
//
// Incorrect IL for lowered thread_local rvalue references
//
// In cases where thread_local variables are lowered (i.e., when
// USE_LAZY_INITIALIZATION_FOR_THREAD_LOCAL_VARIABLES is TRUE), the return value
// for a wrapper routine for an rvalue reference thread local variable didn't
// match the type of the wrapper routine, resulting in invalid IL.  In
// C-generating configurations that use inlining, an internal error ("wrong result
// type for *") had occurred.
thread_local static int&& x(0);
int z = x;
