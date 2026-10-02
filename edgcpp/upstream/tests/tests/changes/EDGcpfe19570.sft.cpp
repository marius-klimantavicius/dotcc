//type:fp
//options_all:--microsoft_version 1914 --ms_c++17
//remark:[5.0] Aggregate class types
// 6/14/18  [EDGcpfe/19570]
//
// Aggregate class types
//
// The front end erroneously failed to disqualify certain class types from being
// aggregate class types.  Specifically, class types with defaulted constructors
// marked "explicit" and class types that inherit virtual functions without
// declaring their own were sometimes treated as aggregate types.
//
// This is now fixed.
struct S {
  explicit S() = default;
  int i;
};
static_assert(!__is_aggregate(S), "Unexpected");  // Previously an error.
                                                  // Now okay.
