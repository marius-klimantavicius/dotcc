//remark:Microsoft using declaration access checking
//options:--microsoft --c++17;fp:--c++17;fn

struct A {
private:
  template <typename T, typename U> static auto bf(T, U bf) -> decltype(bf);
};
struct B : A {};

template <typename T> struct C : T {
  T::bf;
};

template <typename T, typename U>
auto operator|(T arg, U bf) -> decltype(C<U>::bf(arg, bf));

void c() {
  int d = 0;
  d | B();
}
