//type:fp
//options_all:--microsoft_version=1940 --ms_c++20 --no_ms_permissive
//remark:[6.7] Microsoft compatibility: typedef and typename
// 9/23/24  [EDGcpfe/27551,EDGcpfe/27593]
//
// Microsoft compatibility: typedef and typename
//
// In non-permissive Microsoft C++ modes, the front end no longer requires the
// "typename" specifier for a dependent type name if it follows the keyword
// "typedef".
template<class T> auto f(T x) {
  typedef T::X X;  // Ordinarily an error.
                   // Now okay in all Microsoft C++ modes.
  return X(x);
}
