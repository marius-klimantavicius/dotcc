//type:fp
//options_all:--g++
//remark:[4.10.1] GNU compatibility: segfault in lower_gnu_statement_expression
// 4/16/15  [EDGcpfe/16175]
//
// GNU compatibility: segfault in lower_gnu_statement_expression
//
// In certain cases involving destructions of local entities in a GNU statement
// expression, a segfault in lower_gnu_statement_expression could occur.  Now
// fixed.
struct A { ~A(); };
A f() {
  return ({ A a; a; });
}
