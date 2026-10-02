//type:fp
//options_all:--gnu_version 60200
//remark:[5.0] Exception specification of defaulted member established too late
// 3/27/18  [EDGcpfe/17883]
//
// Exception specification of defaulted member established too late
//
// In some cases, the front end did not establish the exception specification of
// a defaulted member in time to produce a correct result for the "noexcept"
// operator.
//
// Previously, this example failed the static assertion because the exception
// specification of D<int>::D() was not established by the time the "noexcept"
// operator in the definition of N is evaluated.  That is now fixed.
template<typename T> struct B { B() noexcept {} };
template<typename T> struct D: B<T> { D() = default; };
template<bool Cond> struct V { static constexpr bool value = Cond; };
struct N: V<noexcept(D<int>())> {};
static_assert(N::value, "Unexpected");
  // Previously failed.  Now okay.
