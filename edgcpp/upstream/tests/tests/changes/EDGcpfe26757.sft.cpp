//type:fp
//options_all:--gn 60000
//remark:[6.6] GNU C++ compatibility: Inheriting from classes with a flexible array member
// 11/2/23  [EDGcpfe/26757]
//
// GNU C++ compatibility: Inheriting from classes with a flexible array member
//
// In GNU C++ mode, the front end previously always diagnosed inheriting from a
// class with a flexible array member when gnu_version >= 60000.  Now, such
// derivations are accepted if the derivation does not introduce data members.
//
// The diagnostic for inheriting from classes with a flexible array member or
// inheriting from a union has also been changed to be more specific.
struct B { int n; int x[]; };
struct D: B {};  // Now accepted in GNU C++ modes.
