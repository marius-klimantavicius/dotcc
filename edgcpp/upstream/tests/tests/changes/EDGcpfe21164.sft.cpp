//type:fp
//options_all:--clang_version 80000 --c++14
//remark:[5.1] Clang compatibility: __reference_binds_to_temporary
// 5/30/19  [EDGcpfe/21164]
//
// Clang compatibility: __reference_binds_to_temporary
//
// In Clang C++ mode with clang_version >= 70000 the front end now supports the
// type traits helper operator "__reference_binds_to_temporary".  Specifically,
// __reference_binds_to_temporary(R, T) produces a true value if and only if R
// is a reference type and
//
// is valid and causes r to be bound to a temporary.
static_assert(!__reference_binds_to_temporary(int&, int&));
static_assert(__reference_binds_to_temporary(int const&, long));
  // Now accepted in some Clang modes.
