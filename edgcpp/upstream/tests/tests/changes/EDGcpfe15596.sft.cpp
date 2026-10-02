//type:fp
//options_all:--c++11
//remark:[6.5] Exception specification of inherited constructor templates
// 2/27/23  [EDGcpfe/15596,EDGcpfe/24928]
//
// Exception specification of inherited constructor templates
//
// Previously, the front end did not generate exception specifications for
// inherited constructor template specializations.
struct B {
  template<typename T> B(T) noexcept;
};
struct D : B {
  using B::B;
};
static_assert(noexcept(D(1)), "Unexpected");  // Previously failed.
                                              // Now okay.
