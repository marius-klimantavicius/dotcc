//type:fp
//options_all:--c++20
//remark:[6.2] Unbounded loop while checking defaulted comparison function
// 11/30/20 [EDGcpfe/23591]
//
// Unbounded loop while checking defaulted comparison function
//
// In some cases, the front end could enter an unbounded loop (in function
// may_be_lvalue_ref_to_const_type) while checking the parameters of a defaulted
// comparison function.
//
// That is now fixed.
template<typename T> struct S {
  friend bool operator !=(S const&, T&) = default;
};                             // Previously triggered an unbounded loop.
