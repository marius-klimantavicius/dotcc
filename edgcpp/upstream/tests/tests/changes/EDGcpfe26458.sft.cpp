//type:fn
//options_all:--gn 120100 --c++20
//remark:[6.6] GNU-mode abort on invalid construct
// 7/6/23   [EDGcpfe/26458]
//
// GNU-mode abort on invalid construct
//
// This example produces several errors, but in some configurations it eventually
// also produced an internal error in extract_node_from_operand (exprutil.c).
// That internal error is now avoided and an ordinary error is issued instead.
template<typename T> struct X {
  struct N { T x; };
  alignas(__alignof__(N::x)) int i; 
};
void f();
X<f()> s;  // Previously sometimes aborted.  Now an ordinary error.
