//type:fp
//options_all:--gnu_version 80100
//remark:[5.1] Trivial copyability and inheriting constructors
// 10/10/18 [EDGcpfe/20252]
//
// Trivial copyability and inheriting constructors
//
// The front end sometimes mistook inheriting constructors for copy constructors
// when determining whether a type is "trivially copyable" (e.g., as part of
// determining whether a class type is a "POD" type).
//
// This is now fixed.
struct B {
  B() noexcept = default;
  B(int) noexcept {}
};
struct D: B {
  D() = default;
  using B::B; 
};
static_assert(__is_pod(D), "Unexpected");  // Previously failed.  Now okay.
