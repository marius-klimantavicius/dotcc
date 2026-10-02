//type:fp
//options_all:--g++
//remark:[4.4] GNU compatibility: Attribute "const" on parameters
// 9/15/11  [EDGcpfe/11716]
//
// GNU compatibility: Attribute "const" on parameters
//
// In GNU mode, the front end now accepts the "const" attribute on parameter
// declarations.
typedef void F();
void g(F *pf __attribute((const)));
