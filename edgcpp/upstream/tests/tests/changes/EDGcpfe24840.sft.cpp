//type:fp
//options_all:--c++20
//remark:[6.3] Nonconstant destruction and constinit variables
// 11/11/21 [EDGcpfe/24840]
//
// Nonconstant destruction and constinit variables
//
// The front end previously issued an error for constinit variables whose
// initialization is constant but whose destruction is not constant.
//
// That is now fixed.  In cases like these, the initializer for the variable will
// point to a dynamic initialization entry (a_dynamic_init), but that entry will
// represent constant initialization (e.g., it will have kind dik_constant).
struct X { int i; ~X(); };
constinit X x{ 42 };  // Previously an error.  Now okay.
