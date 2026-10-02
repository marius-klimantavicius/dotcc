//type:fp
//options_all:--microsoft
//remark:[4.3] Microsoft compatibility: Redeclarations changing linkage
// 10/18/10 [EDGcpfe/11072,EDGcpfe/11079]
//
// Microsoft compatibility: Redeclarations changing linkage
//
// In Microsoft mode, the front end now accepts a redeclaration with internal
// ("static") linkage inside an extern "C" block following a previous declaration
// for the same entity that resulted in external linkage (a warning is issued).
void f();  // External linkage
extern "C" { static void f() {} }
           // Internal linkage: Conflict is now accepted with a warning.
