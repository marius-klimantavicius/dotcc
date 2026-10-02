//type:fp
//options_all:--c++11 --gnu_version=80100
//remark:[6.3] Clang and GNU C++ compatibility: Mem-initializers for flexible array members
// 3/9/21   [EDGcpfe/24034]
//
// Clang and GNU C++ compatibility: Mem-initializers for flexible array members
//
// The changes for EDGcpfe/22387 in version 6.2 of the front end introduced a
// regression: Those changes resulted in an error being issued if a flexible
// array member of a class type was value- or default-initialized by the
// mem-initializers of a constexpr constructor.
//
// Now, such cases are accepted in Clang C++ mode when clang_version >= 11000 and
// in GNU C++ mode when gnu_version >= 80000 and gnu_version < 100000.
struct S {
  constexpr S()
    : i(), fam() {} // An error in version 6.2.  Now sometimes okay.
  int i;
  char fam[];
} s;
