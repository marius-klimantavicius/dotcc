//type:fp
//options_all:--c++17
//remark:[5.0] C++-generating back end: Extraneous ellipsis in rendering of fold expressions
// 6/26/18  [EDGcpfe/19796]
//
// C++-generating back end: Extraneous ellipsis in rendering of fold expressions
//
// The C++-generating back end previously sometimes generated extraneous ellipses
// when rendering certain C++17 fold expressions.
//
// Previously, the deduction guide was rendered as follows by some configurations
// of the C++-generating back end:
//
// That is now fixed.
template<typename> bool V = false;
template<bool> struct S {};
template<typename ...Ts> S(Ts...) -> S<(V<Ts> && ...)>;
