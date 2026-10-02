//type:fp
//options_all:--c++17
//remark:Spurious parsing error on parenthesized lambda in template declaration
// 5/20/26  [EDGcpfe/28859]
//
// Spurious parsing error on parenthesized lambda in template declaration
//
// The front end attempted to cache the tokens of the default template argument,
// but did so incorrectly and issued a number of spurious errors as a result of
// the incorrect caching.  That is now fixed.
template<int I = ([]{ if constexpr (true) return 42; } )()>
int f() { return I; }
