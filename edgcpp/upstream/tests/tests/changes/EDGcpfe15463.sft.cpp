//type:fp
//options_all:--microsoft
//remark:[4.10] Microsoft compatibility: Bound functions in unevaluated contexts
// 9/19/14  [EDGcpfe/15463]
//
// Microsoft compatibility: Bound functions in unevaluated contexts
//
// A "bound function" is an expression of the form "obj.f" or "ptr->f" where f is
// a nonstatic member function.  Ordinarily, such an expression can only be used
// to call the indicated member function.  Now, in Microsoft mode, the front end
// also allows taking the address of such an expression inside a sizeof operand:
// "&(p->f)" is treated as a pointer-to-function in such cases.
//
// The bound function in such cases is represented by an eok_points_to_static or
// eok_dot_static node (this is a new use of those operators).
struct S { int f(); };
unsigned s = sizeof(&((S*)0)->f);  // Now accepted in Microsoft mode.
