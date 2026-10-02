//type:fp
//options_all:--c++14
//remark:[4.9] C++14: decltype(auto)
// 2/5/14   [EDGcpfe/14383]
//
// C++14: decltype(auto)
//
// In C++14 mode, the front end now accepts the decltype(auto) construct for
// variable declarations.
decltype(auto) i1 = 1;     // deduces i1 to be int (i.e., decltype(1)).
decltype(auto) i2 = (i1);  // deduces i2 to be int& (i.e., decltype((i1))).
decltype(auto) i3 = i1;    // deduces i3 to be int (i.e., decltype(i1)).
