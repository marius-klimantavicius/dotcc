  template<typename T> class X {
    ~X();
  };
  template<typename> void f(X<char> v) {
    static_assert([v]{return true;}(), "Expected");
  }
