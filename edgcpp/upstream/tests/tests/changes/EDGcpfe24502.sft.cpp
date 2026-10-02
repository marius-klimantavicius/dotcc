//type:fp
//options_all:--gnu=70000 --c++11
//remark:GCC/Clang compatibility: Character zeros as null pointer constants
// 7/9/26   [EDGcpfe/24502,EDGcpfe/27855,EDGcpfe/28923]
//
// GCC/Clang compatibility: Character zeros as null pointer constants
//
// In C++11 and later modes, GCC 7 and later no longer treat a zero of character
// type (for example, (unsigned char)0 or '\0') as a null pointer constant, and
// Clang never did.  The front end previously still treated such expressions as
// null pointer constants in GNU and Clang C++ modes, which could lead to
// spurious errors.
//
// That is now fixed.  (Microsoft modes retain the prior behavior to match MSVC.)
// In GNU C++ modes, casting a zero literal to a non-character integer type (for
// example, (int)0) can still produce a null pointer constant, matching g++.
struct X {
  void operator+=(char*);
  void operator+=(char);
};
void g(X &msg) {
  msg += (unsigned char)0;  // Previously ambiguous; now okay.
}
