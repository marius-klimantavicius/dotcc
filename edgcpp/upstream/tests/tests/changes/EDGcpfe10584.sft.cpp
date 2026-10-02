//type:fp
//options_all:--c++11
//remark:[4.8] C++11: reinterpret_cast and identical integer/enum types
// 8/8/13   [EDGcpfe/10584]
//
// C++11: reinterpret_cast and identical integer/enum types
//
// In C++11 mode, the front end now accepts a reinterpret_cast that casts a
// source expression of integer or enumeration type to that same type.
//
// This implements the C++ standards committee's Core issue 799 resolution.
int p = 1, q = reinterpret_cast<int>(p);  // Now accepted in C++11 mode.
