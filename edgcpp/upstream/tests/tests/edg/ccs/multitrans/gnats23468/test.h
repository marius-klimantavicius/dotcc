template<typename T> struct Empty {};

template<typename T> struct Bad {
  template <typename... l> Bad() noexcept(Empty<l...>::val2);
};

template<typename T> struct S1 {
  S1();
  Bad<T> ag;
};

template<typename T> void func() {
  S1<char> var;
}

template<typename T> class S2;
template<> class S2<char> {
  void foo() {
    func<char>();
  }
};

template<typename T>
struct X {
  X() {
    S1<char> var;
  }
};
