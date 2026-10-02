//type:fp
//options_all:--c++11
//remark:[4.10.1] Dependent expressions in constant contexts
// 2/11/15   [EDGcpfe/16010]
//
// Dependent expressions in constant contexts
//
// The front end previously incorrectly rejected some expressions appearing in
// constant contexts in template definitions as not having a constant value if
// they involve dependent values.  This is now fixed.
// --c++11:
template <int I> struct Int { };

template <int I, int J>
constexpr int operator+(Int<I> lhs, Int<J> rhs) { return I + J; }

template <int I, int J>
struct Sum : Int<Int<I>() + Int<J>()> { };  // Previously a spurious error
