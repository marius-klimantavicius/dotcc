//type:fp
//options_all:--c++23
//remark:[6.6] C++23: Add support for attributes in lambda-expressions
// 9/15/23  [EDGcpfe/25076,EDGcpfe/26487]
//
// C++23: Add support for attributes in lambda-expressions
//
// The C++23 standard allows attributes in lambda-expressions; such attributes
// appertain to the function call operator of the lambda.  These attributes are
// now accepted in C++23 mode as well as in later GNU and Clang emulation modes.
//
// See WG21 document P2173R1 for more information.
auto a = [] [[noreturn]] (auto z) { throw z; };
auto b = [] <class T> [[noreturn]] (T z) { throw z; };
