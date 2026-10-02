//type:fp
//options_all:--ms_c++20
//remark:[6.6] Microsoft compatibility: partial ordering of constrained function templates
// 7/28/23  [EDGcpfe/26514]
//
// Microsoft compatibility: partial ordering of constrained function templates
//
// According to the rules in [temp.func.order], constraints should only be
// considered if deduction succeeded both ways as described in
// [temp.deduct.partial].  However, the Microsoft compiler appears to also
// front end now emulates that behavior in Microsoft mode.
template<typename T, typename U>
void f(T *, U);
template<typename T, typename U>
char f(T, U *) requires true;
char c = f("", "");  // Normally ambiguous.  Now okay in Microsoft mode.
