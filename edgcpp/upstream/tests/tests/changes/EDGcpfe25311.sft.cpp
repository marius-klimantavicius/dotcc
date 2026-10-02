//type:fp
//options_all:--gn 110300 --c++17
//remark:[6.4] GNU C++11 compatibility: __extension__ prefix for member using-declaration
// 5/13/22  [EDGcpfe/25311,EDGcpfe/25328]
//
// GNU C++11 compatibility: __extension__ prefix for member using-declaration
//
// In GNU C++11 mode, the front end now accepts the __extension__ prefix for
// member using-declarations.
struct S {
  __extension__ using T = int;  // Previously an error.  Now accepted in
};                              // GNU C++11 modes.
