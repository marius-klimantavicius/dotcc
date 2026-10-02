//type:fp
//remark:[5.0] C++-generating back end: aggregate initialization of a reference
// 2/28/18  [EDGcpfe/17745]
//
// C++-generating back end: aggregate initialization of a reference
//
// The C++-generating back end previously failed an assertion (in
// gen_initializer_constant) when a reference is initialized with an aggregate
// constant.  This is now fixed.
typedef const char (&T)[4];
T r = T{};   // Previously caused an assertion failure
