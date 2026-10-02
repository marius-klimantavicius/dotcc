//type:fp
//options_all:--clang_v 80000 --c
//remark:[6.8] C23: Explicit underlying enum types
// 10/9/25  [EDGcpfe/24604,EDGcpfe/25943,EDGcpfe/27793,EDGcpfe/27833,
//           EDGcpfe/28149,EDGcpfe/28468]
//
// C23: Explicit underlying enum types
//
// In C23 mode, the front end now accepts an explicit underlying type for the
// declaration of an enumeration type.  (This is similar to a C++11 feature.)
//
// This is also accepted in all Clang C modes with clang_version >= 80000 and in
// all GNU C modes with gnu_version >= 130000 (i.e., not just in C23 modes).
// This feature was added to C23 through the C standardization committee's paper
// N3030.
enum Answer: unsigned char { No, Yes, Maybe };
