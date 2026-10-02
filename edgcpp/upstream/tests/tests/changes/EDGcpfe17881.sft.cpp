//type:fn
//options_all:--c++17
//remark:[4.13] Ill-formed nested namespace declaration
// 12/23/16 [EDGcpfe/17881,EDGcpfe/17879]
//
// Ill-formed nested namespace declaration
//
// An assertion failure ("pop_scope: curr_construct_pragmas != NULL") had been
// given on certain ill-formed nested namespace declarations and now an error
// is given.
namespace A::B = A;
