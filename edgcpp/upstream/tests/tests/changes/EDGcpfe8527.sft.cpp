//type:fp
//options_all:--c++11
//remark:[4.8] C++11: alignas and alignof
// 6/26/13  [EDGcpfe/8527]
//
// C++11: alignas and alignof
//
// In C++11, the front end now accepts alignas(...) as an alignment specifier
// (equivalent to the early draft-standard [[align(...)]] attribute; see the
// entry of 12/3/09 for EDGcpfe/8293) and alignof(...) to query the alignment
// of a type (similar to the existing __alignof__ extension).
//
// The C++11 standard doesn't allow an expression argument for "alignof", but
// since it is a common extension, the front end does accept expression
// arguments with a warning in non-strict C++11 modes:
struct alignas(16) S {};
static_assert(alignof(S) == 16, "Error");
