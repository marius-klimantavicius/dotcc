//type:fp
//options_all:--c++11
//remark:[4.10] Nontype template arguments and linkage
// 9/23/14  [EDGcpfe/11269,EDGcpfe/14191,EDGcpfe/15139]
//
// Nontype template arguments and linkage
//
// C++11 relaxed the requirements for nontype template arguments to allow
// references and pointers to functions and variables with external linkage.  The
// front end now allows this in C++11 mode (or, more precisely, when the global
// variable local_types_as_template_args_enabled is TRUE; this includes certain
// Microsoft C++ modes).
//
// (This relaxation came about through the C++ standards committee's resolution
// for Core issue 1155.)  In Microsoft mode, this relaxation also applies to
// static member functions of local classes (which have no linkage at all).  For
// example:
template<int *p> struct S {};
static int i = 0;
S<&i> si;  // Now permitted in C++11 mode and in some Microsoft modes.
