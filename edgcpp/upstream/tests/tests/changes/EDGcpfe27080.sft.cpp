//type:fp
//options_all:--clang_v 180000
//remark:[6.7] Clang C++ compatibility: __datasizeof
// 3/6/24   [EDGcpfe/27080]
//
// Clang C++ compatibility: __datasizeof
//
// In Clang C++ modes with clang_version >= 180000, the front end now supports the
// __datasizeof operator, which acts like the traditional sizeof operator except
// that for the IA-64 ABI it doesn't include tail padding for some class types.
//
// No mangling is provided for this operation (since Clang appears not to have
// implemented that at this time).
struct S {
  S();  // The constructor enables the tail padding exemption.
  long long l;
  char c;
};
static_assert(sizeof(S) == 16);
static_assert(__datasizeof(S) == 9);
