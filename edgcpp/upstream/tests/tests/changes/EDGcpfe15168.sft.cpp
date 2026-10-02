//type:fp
//options_all:--c++14 --gnu=70200
//remark:[5.1] String literal operator templates
// 10/3/18  [EDGcpfe/15168,EDGcpfe/20235]
//
// String literal operator templates
//
// C++ Committee document P0424R0 proposed the addition of string literal
// operator templates to the language.  Although this feature was not adopted
// into the C++ Standard, g++ and clang have implemented it as an extension.
// The front end now also supports this feature in C++14 and newer g++ and
// clang modes with gnu_version >= 40900.
// --gnu_version=70200:
template<typename T, T ...chars> int operator"" _x(); 
int i = L"ab"_x;  // calls operator""_x<const wchar_t, L'a', L'b'>()
