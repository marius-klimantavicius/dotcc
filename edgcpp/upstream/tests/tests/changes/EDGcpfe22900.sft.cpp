//type:fp
//options_all:--c++20
//remark:[6.2] Partial ordering of rewritten comparison candidates
// 11/18/20 [EDGcpfe/22900]
//
// Partial ordering of rewritten comparison candidates
//
// The resolution of Core issue 2445 fixed the rules determining the partial order
// of function templates when one of the candidates is a C++20 comparison function
// with reversed parameters.  The front end now implements that resolution.
template<typename T> struct X {};
template <typename T, typename U> bool operator==(T, X<U*>);
template <typename T, typename U> bool operator!=(X<T>, U) = delete;
auto r = X<int*>{} != X<int>{};  // Previously an error.  Now okay.
