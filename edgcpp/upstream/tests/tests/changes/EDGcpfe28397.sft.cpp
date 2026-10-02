//type:fp
//options_all:--c++20
//remark:Abort on checking of nested constraints
// 1/7/26   [EDGcpfe/28397,EDGcpfe/28632]
//
// Abort on checking of nested constraints
//
// With the changes for EDGcpfe/27599 (in version 6.7), the front end could abort
// in some cases due to a failed internal assertion in get_expr_rescan_info when
// checking nested constraints.
template<int> struct C {
  static constexpr bool v = true;
};
template<int I> struct B
{
  template<int> struct D {
    static int f(auto) requires(C<I>::v);  // Previously aborted.  Now okay.
  };
};
int i = B<1>::D<2>::f(3);
