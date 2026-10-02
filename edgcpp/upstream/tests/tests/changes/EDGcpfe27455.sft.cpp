//type:fp
//options_all:--microsoft_v 1940 --ms_c++20
//remark:[6.7] Microsoft-mode abort assigning a braced construct to a property field
// 7/30/24  [EDGcpfe/27455]
//
// Microsoft-mode abort assigning a braced construct to a property field
//
// Previously, this elicited an internal error in alloc_arg_list_elem_for_operand
// (in exprutil.c).  That is now fixed.
struct S {
  void f(int);
  __declspec(property(put = f)) int prop;
};
void g(S &s) {
  s.prop = {};  // Previously aborted.  Now okay.
}
