//type:fp
//remark:[4.3] Abort on instantiation of function template with noalias or restrict attribute
// 9/16/10  [EDGcpfe/10977]
//
// Abort on instantiation of function template with noalias or restrict attribute
//
// The new attributes framework of version 4.2 introduced a bug causing the front
// end to abort with an internal error in update_routine_modifiers (decls.c)
// during the instantiation of a function template declared with the Microsoft
// __declspec attributes "noalias" or "restrict".
//
// This is now fixed.
template<typename T> __declspec(noalias) void f(T) {}
template void f(int);  // Previously triggered an internal error.
