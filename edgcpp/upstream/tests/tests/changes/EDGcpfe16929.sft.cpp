//type:fp
//options_all:--c++11
//remark:[4.12] constexpr reference cast to same type
// 7/29/16  [EDGcpfe/16929]
//
// constexpr reference cast to same type
//
// The front end previously issued a spurious error for a cast of a constant
// lvalue to a reference to the same type that appeared in a context requiring
// a constant expression.  This is now fixed.
constexpr int i = 2;
constexpr int j = static_cast<const int&>(i);  // Previously an error,
                                               // now okay
