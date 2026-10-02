//type:fp
//options_all:--c++20 --gnu_version 110100 -tused -w
//remark:[6.4] Nonconstant destruction and constinit variables
// 6/18/22  [EDGcpfe/24657,EDGcpfe/25409]
//
// Nonconstant destruction and constinit variables
//
// The changes for EDGcpfe/24840 (in version 6.3) were incomplete in that they
// didn't handle default initialization of constinit variables with nonconstant
// destruction.
//
// That oversight is now fixed.
struct S { ~S(); };
constinit S s1{};  // Accepted since version 6.3.
constinit S s2;    // Previously a spurious error.  Now okay.
