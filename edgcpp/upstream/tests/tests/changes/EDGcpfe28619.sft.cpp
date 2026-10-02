//type:fp
//options_all:--c++23
//remark:Lambda-vs-designator disambiguation with lambda attributes
// 1/6/26   [EDGcpfe/28619]
//
// Lambda-vs-designator disambiguation with lambda attributes
//
// The front end previously treated the first bracket ("[") in the initializer of
// r as the beginning of an array designator.  This was caused by the presence of
// attributes following the leading "[]" and resulted in spurious syntax errors.
// The disambiguation for this case has now been fixed to correctly handle lambda
// attributes (a C++23 feature; see the Changes for EDGcpfe/25076,EDGcpfe/26487
// in version 6.6).
struct F { template<typename T> F(T) {} };
auto r = F{ [] [[]] () {} };  // Previously an error.  Now okay.
