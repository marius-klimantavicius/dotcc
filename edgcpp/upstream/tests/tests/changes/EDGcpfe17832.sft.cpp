//type:fp
//options_all:--c++11
//remark:[4.13] Core issue 2076
// 12/13/16 [EDGcpfe/17832]
//
// Core issue 2076
//
// The front end now implements the resolution of Core issue 2076 by default
// (previously, it effectively already did so in Clang and GNU modes).  This
// change is a tweak on the resolution of Core issue 1467 (see the changes for
// EDGcpfe/16425) that disables user-defined conversions when considering a
// copy or move constructor (and certain similar constructor contexts) and
// matching it to a braced initializer that contains a single element that is
// itself a braced initializer.
//
// Previously, this was ambiguous because the initializer could be interpreted
// as "B b{A{0}};" or "B b{B{A(0)}};".  Now, the latter interpretation is
// disabled.
struct A { A(int); };
struct B { B(A); };
B b{{0}};
