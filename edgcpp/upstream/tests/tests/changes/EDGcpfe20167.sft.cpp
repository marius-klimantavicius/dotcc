//type:fp
//options_all:--c++17
//remark:[5.1] Copy elision with the comma operator
// 4/26/19  [EDGcpfe/20167]
//
// Copy elision with the comma operator
//
// The front end was not eliding copies when the result of the comma operator
// was eligible for copy elision.
//
// This is now fixed.
struct S { S(); S(const S&) = delete; };
S s = (42, S{});  // Spurious deleted copy constructor error issued
