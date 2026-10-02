//type:fp
//options_all:--microsoft_v 1928
//remark:[6.7] Microsoft compatibility: undeclared base class of class template
// 5/13/24  [EDGcpfe/24350,EDGcpfe/27184]
//
// Microsoft compatibility: undeclared base class of class template
//
// In Microsoft permissive mode, the base class of a class template can still be
// undeclared at the point of template definition.
template<typename T>
struct C : B {};  // Now accepted in Microsoft permissive mode.
struct B {};
