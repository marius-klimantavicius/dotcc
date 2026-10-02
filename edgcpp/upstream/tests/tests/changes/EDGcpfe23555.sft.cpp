//type:fp
//options_all:--c++23
//remark:[6.5] C++23: "z" integer literal suffix
// 2/17/23  [EDGcpfe/23555]
//
// C++23: "z" integer literal suffix
//
// As described in WG21 paper P0030R8, the front end now accepts in C++23 mode
// the integer literal suffix "z" or "Z", giving the literal the type
// std::size_t (if the suffix also includes "u" or "U") or the signed integer
// type corresponding to std::size_t (if the "u/U" suffix is omitted).  The
// "z/Z" suffix is also accepted in GNU and clang C++ modes with gnu_version
// >= 110000 or clang_version >= 130000, respectively.
// --c++23:
static_assert(sizeof(1uz) == sizeof(sizeof(0)));
