//type:fp
//options_all:--c++23
//remark:Trailing return type in C++23 lambda without a parameter declaration clause
// 3/4/26   [EDGcpfe/28695]
//
// Trailing return type in C++23 lambda without a parameter declaration clause
//
// Previously, the front end failed to parse a C++23 lambda expression with a
// trailing return type that immediately follows a requires clause.
// with --c++23:
auto l = []<typename> requires true -> void { };  // Previously a spurious
                                                  // error.  Now okay.
