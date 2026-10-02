//type: fn
//options:  --c++03 --no_strict_gnu
// { dg-options "-std=gnu++98" }
template <class T >
struct S
{
  S() : S() {} // { dg-message "delegating constructors" }
}; // { dg-error "delegates to itself" "" { target *-*-* } .-1 }

S<int> s;
