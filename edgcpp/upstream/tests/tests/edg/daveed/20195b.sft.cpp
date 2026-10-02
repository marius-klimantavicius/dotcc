//remark:Substitution of templates
//options:--c++14;fp:--gnu=80200 -w -tused;fp

struct Z { static int const value = 0; };
template<bool> struct B {};
 
struct R {
  template<typename ...Ts> Z operator()(Ts&& ...) const;
};
 
struct A {
  template<typename X> auto operator()(X &&x) const {
    return R{}(x);
  }
};
 
template <typename P>
struct D {
  template<typename T>
    auto f(T &&p)-> B<decltype(P{}(p))::value>;
};
 
void g() {
   D<A> d;
   d.f(Z{});
}
