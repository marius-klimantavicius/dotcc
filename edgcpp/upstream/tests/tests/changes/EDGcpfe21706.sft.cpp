//type:fp
//options_all:--gn 90100 --c++17
//remark:[6.0] __is_trivially_copyable for unions with non-trivially copyable members
// 9/4/19   [EDGcpfe/21706]
//
// __is_trivially_copyable for unions with non-trivially copyable members
//
// The change for EDGcpfe/21184 (in version 5.1) inadvertently introduced a
// regression in the evaluation of __is_trivially_copyable for unions with
// non-trivially copyable members.
//
// This is now fixed.
struct S {
  S& operator=(const S&);
};
union U {
  S s;
};
static_assert(!__is_trivially_copyable(S));
static_assert(!__is_trivially_copyable(U)); // Would previously fail
