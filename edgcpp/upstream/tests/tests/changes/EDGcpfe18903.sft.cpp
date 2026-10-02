//type:fp
//options_all:--gnu_version 80000
//remark:[5.0] GNU C++ compatibility: __integer_pack
// 1/17/18  [EDGcpfe/18903]
//
// GNU C++ compatibility: __integer_pack
//
// In GNU C++ modes with gnu_version >= 80000 the front end now accepts template
// arguments of the form "__integer_pack(N)" where N is a nonnegative integer
// constant.  Such a construct expands to template arguments 0, 1, ..., N-1 (an
// empty list of arguments if N is zero).
//
// GCC uses this feature in the standard <utility> header.  Our implementation
// is slightly different from GCC's because of internal representation issues,
// but it supports the use cases intended by the GCC feature.
template<short ... N> struct B {};
template<int N> int f(B<__integer_pack(N)...>) { return 1; }
int r = f<3>(B<0, 1, 2>{});  // Okay.
