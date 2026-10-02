//type:fp
//options_all:--microsoft
//remark:[4.10.1] Microsoft mode abort on binding of nontype template reference argument
// 3/11/15  [EDGcpfe/16083]
//
// Microsoft mode abort on binding of nontype template reference argument
//
// The changes for EDGcpfe/15084 etc. (see entry of 6/10/14) introduced a
// regression in Microsoft mode that could trigger an internal error in exprutil.c
// (function take_reference_to_operand) when a nontype template reference
// argument is bound to a nontype template reference parameter.
//
// This is now fixed.
struct V {};
template<const V &Val> struct A {};
template<const V &Val> struct B: A<Val> {};
template<const V &Val> struct C: B<Val> {};
  // Previously triggered an abort in Microsoft mode during the prototype
  // instantiation of B<Val>.
