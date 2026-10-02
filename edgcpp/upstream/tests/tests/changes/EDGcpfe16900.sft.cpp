//type:fp
//options_all:--c++11
//remark:[4.12] constexpr copy construction from value-initialized members
// 9/12/16  [EDGcpfe/16900]
//
// constexpr copy construction from value-initialized members
//
// The front end previously reported a spurious "must have a constant value"
// error for cases in which bitwise copy initialization is required from a
// value-initialized class member in a context requiring a constant
// expression.  This is now fixed.
//
// 5/24/16  [EDGcpfe/16900]
//
// Folding bitwise-copied class members
//
// The front end previously failed to fold an invocation of a compiler-generated
// copy constructor for a class if one or more of the members involved a
// bitwise copy.  This is now fixed.
template <typename... Ts> struct A;
template <> struct A<> {
  constexpr A() { }
  constexpr A(const A&) { }
};
template <typename First, typename... Rest>
struct A<First, Rest...> : A<Rest...> {
  typedef A<Rest...> Base;
  constexpr A() : Base(), elem() { }
  First elem;
};
constexpr A<int, int, int> t1;
constexpr A<int, int, int> t2(t1);  // Previously a spurious error
