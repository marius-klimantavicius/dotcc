//type:fp
//options_all:--gnu=140100
//remark:[6.7] GNU C++ compatibility: bf16 floating-point suffix
// 5/14/24  [EDGcpfe/27240,EDGcpfe/27246]
//
// GNU C++ compatibility: bf16 floating-point suffix
//
// Although formally only part of C++23, g++ began accepting the bf16
// floating-point suffix in all language versions beginning with version 13.1,
// and the headers released with gcc version 14.1 include a use of that
// suffix.  The front end has now been updated to accept this usage in g++
// mode when gnu_version is at least 130000.
// --gnu_version=140100:
typedef __decltype(0.0bf16) __bfloat16_t;   // Previously an error, now okay
