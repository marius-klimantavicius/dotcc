//type:fp
//options_all:--strict --c++20
//remark:[6.8] Friends defined in class template instantiations and constexpr requirements
// 10/28/25 [EDGcpfe/24748,EDGcpfe/26366,EDGcpfe/28510,EDGcpfe/28511]
//
// Friends defined in class template instantiations and constexpr requirements
//
// Certain failing constraints on constexpr functions -- such as the requirement
// that their return type be a literal type -- do not result in an error if the
// failure is on the instance of a templated function; instead, that instance is
// silently handled as a non-constexpr function.  The front end now treats
// constexpr friend functions defined in class templates in the same way.
//
// Previously, this example complained about the return type of f(X<int>) not
// being a literal type.  Now the example is accepted.
//
// 7/1/22   [EDGcpfe/23306,EDGcpfe/24748,EDGcpfe/25063]
//
// Literal types and constexpr friends of class templates
//
// The C++ standard makes this invalid because f(X) is declared constexpr but its
// return type X is not a literal type.  The standard makes an exception for
// instances of function templates and for members of class template instances,
// but this is neither of those cases.  It is expected that the committee will
// eventually relax that rule and, in nonstrict modes, the front end now issues
// only a warning on this case.
struct S { S(); };  // Not a literal type.
template<typename T> S g(T);
template<typename T> struct X {
  friend constexpr auto f(X<T> x) {
    return g(x);
  }
};
X<int> xi;
auto r = f(xi);
