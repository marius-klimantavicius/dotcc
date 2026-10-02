//type:fp
//remark:[6.1] Incorrect inheriting constructor template instantiation context scopes
// 3/19/20  [EDGcpfe/19781,EDGcpfe/22052]
//
// Incorrect inheriting constructor template instantiation context scopes
//
// When inheriting constructor templates are being instantiated, the front end was
// using the scope of the derived class as the context scope, rather than the
// scope of the base class from where the constructor originated.  This could
// cause spurious instantiation failures (and therefore cause SFINAE to behave
// incorrectly).
//
// This is now fixed.
struct S {};
template<typename, typename> struct X {};
template<typename> struct A {
  static constexpr bool val = true;
};
template<bool, typename = void> struct B;
template<typename T> struct B<true, T> { typedef T type; };
template<typename... T> struct C : S {
  C(const T&...) = delete; // Previously the front end chose this function
  template<typename... U,
           typename = typename B<A<X<T, U>...>::val>::type
          >
  C(U&&...); // Now the front end chooses this function
};
struct D : C<S> {
  using C::C;
};
void f(S s) {
  D var(s); // Previously an error (calling deleted function), now accepted
}
