//type:fp
//options_all:--g++ --c++17
//remark:[6.2] Abort on value-initialization of GNU vector type
// 11/19/20 [EDGcpfe/21953,EDGcpfe/23506,EDGcpfe/23604]
//
// Abort on value-initialization of GNU vector type
//
// The front end previously aborted (with an internal error in overload.c) on
// attempts to value-initialize a GNU vector type.
//
// That is now fixed.
typedef int V4I __attribute((vector_size(16)));
void f(V4I);
void g() { f({}); }  // Previously triggered an internal error.  Now okay.
