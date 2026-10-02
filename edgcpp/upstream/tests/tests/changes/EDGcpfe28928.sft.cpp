//type:fp
//options_all:--clang_v 220100 --c++03
//remark:GNU and Clang C++ compatibility: variable templates in C++03 mode
// 7/10/26  [EDGcpfe/28928]
//
// GNU and Clang C++ compatibility: variable templates in C++03 mode
//
// The changes for EDGcpfe/27938,EDGcpfe/27974 (in version 6.8) enabled variable
// templates in some GNU and Clang C++11 modes (with a warning).  It turns out
// that the corresponding versions of GCC and Clang also accept variable templates
// in their C++03 modes.  The front end now matches that behavior
template<class T> const bool is_floating_v = false;
template<> const bool is_floating_v<float> = true;
bool b = is_floating_v<float>;
