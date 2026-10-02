//remark:Access checking and SFINAE through using-declarations
//options:--c++17;fp

struct X {
  template<typename T> struct N: T {
    using T::f;
  };
};
class C {
  friend struct X;
  static int f();
};
template<typename T> auto g(T) -> decltype(X::N<T>::f());
int main() {
  g(C{});
}
