//type:fp
//options_all:--c++14
//remark:[6.6] Abort on in-class explicit specialization with deduced type
// 8/25/23  [EDGcpfe/26624]
//
// Abort on in-class explicit specialization with deduced type
//
// The changes for EDGcpfe/25751 (in version 6.5) introduced a regression for an
// in-class explicit specialization of a static data member template with a
// deduced type.  Previously, this triggered an internal error in
// generic_cast_operand (exprutil.c).
struct C {
  template<typename T> static const auto v = false;
  template<> const auto v<int> = true;  // Previously aborted.  Now okay.
};
