//type:fp
//options_all:--c++20
//remark:[6.6] Abort on CTAD with non-type template parameter in requires clause
// 7/6/23   [EDGcpfe/26435]
//
// Abort on CTAD with non-type template parameter in requires clause
//
// Previously, a requires clause referring to a non-type template parameter could
// cause an internal error in transfer_arg_operand_for_template_arg (expr.c)
// during class template argument deduction.
template<int> concept C = false;
template<typename T, int I = 0>
struct D {
  D(T) requires (!C<I>);  // Previously triggered an internal error.
};                        // Now okay.
D d(0);
