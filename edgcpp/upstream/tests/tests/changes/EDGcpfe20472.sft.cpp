//type:fp
//options_all:--microsoft_version 1915
//remark:[5.1] Microsoft compatibility: MSVC allows const_cast to a function type
// 11/26/18 [EDGcpfe/20472]
//
// Microsoft compatibility: MSVC allows const_cast to a function type
//
// In Microsoft mode, the front end now allows const_cast to be applied to a
// function type.
int foo(int*);
auto bar = const_cast<int(*)(int*const)>(foo);
