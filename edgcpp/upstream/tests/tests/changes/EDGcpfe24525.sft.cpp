//type:fp
//options_all:--gn 50500 --c++14
//remark:[6.3] Internal error on substitution of static_cast involving default argument
// 8/3/21   [EDGcpfe/24525]
//
// Internal error on substitution of static_cast involving default argument
//
// In some cases, rescanning an operand that was originally a non-dependent
// static_cast relying on a constructor with a default argument resulted in an
// internal error in make_cast_rescan_operands (exprutil.c).
//
// That is now fixed.
struct X { X(int, int = 1); };
template<typename T> decltype(T{}, static_cast<X>(2)) f(T&&);
auto r = f(3);  // Previously triggered an abort.  Now okay.
