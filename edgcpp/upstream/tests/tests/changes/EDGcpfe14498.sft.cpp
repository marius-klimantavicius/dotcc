//type:fp
//options_all:--gnu=50000 --c++11
//remark:[4.11] GNU compatibility: folding constant vector arithmetic expressions
// 1/22/16  [EDGcpfe/14498,EDGcpfe/15550,EDGcpfe/16781]
//
// GNU compatibility: folding constant vector arithmetic expressions
//
// The front end previously aborted with an assertion failure in
// unary_operation or binary_operation if an attempt was made to fold a
// constant vector arithmetic expression.  This is now fixed.
// with --gnu_version=50000 --c++11:
typedef int __attribute__((vector_size (4*sizeof(int)))) vectype;
void f() {
  constexpr vectype x {};
  constexpr vectype y {1,2};
  constexpr vectype z = x + y;   // Previously aborted, now z == {1,2,0,0}
}
