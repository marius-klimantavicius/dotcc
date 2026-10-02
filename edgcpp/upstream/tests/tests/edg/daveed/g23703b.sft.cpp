//remark:Variable instantiations in SFINAE
//options:--c++20 -tused;fp

  template<typename T>
    concept C1 = requires(T t) { t.f(); };
  template<typename T> requires C1<T&> void f(T&&);
  template<typename T>
    concept C2 = requires(T &t) { f(t); };
  template<typename T> struct B {
    void g() requires C2<T>;  // (1)
  };
  struct D: B<D> {  // (2)
    int f();
  };
  int main() {
    f(D{});  // (3)  Previously an error.  Now okay.
  }

