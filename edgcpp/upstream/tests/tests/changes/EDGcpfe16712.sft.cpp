//type:fp
//options_all:--c++17
//remark:[4.12] C++17 compatibility: Add nested namespace definitions
// 5/17/16  [EDGcpfe/16712,EDGcpfe/16713]
//
// C++17 compatibility: Add nested namespace definitions
//
// The front end now accepts nested namespace definitions as outlined in N4230
// in C++17 mode as well as when microsoft_version >= 1903.
namespace A::B::C {}
// Equivalent to: "namespace A { namespace B { namespace C {}}}"
