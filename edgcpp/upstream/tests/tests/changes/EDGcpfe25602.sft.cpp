//type:fp
//options_all:--g++ --c++20
//remark:[6.4] GNU C++ compatibility: array designated initializers in C++20
// 9/6/22   [EDGcpfe/25602]
//
// GNU C++ compatibility: array designated initializers in C++20
//
// The C++20 Standard supports designated initializers for non-static data
// members but not for array elements; see EDGcpfe/20003,EDGcpfe/20119.
// However, g++ in C++20 mode does not enforce this restriction, and the front
// end has now been changed to follow suit in the corresponding emulation mode.
int x[] = { [0] = 5 };   // Previously an error, now accepted
